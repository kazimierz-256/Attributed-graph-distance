# scrape from http://www.enron-mail.com/email/
# Kazimierz Wojciechowski
import socket
import urllib.parse
import urllib.request
import re
import datetime
from random import shuffle

import dateutil.parser
import json
import os

from sphinx.util import requests


def parse_directory(url, content, check_for_folder_cached, notify_new_folder, percentage_begin, percentage_amount):
    matches = re.findall(r"href=\"([\w-]+?/)\"", content)
    percentage_local_begin = percentage_begin
    percentage_local_amount = percentage_amount / len(matches)
    shuffle(matches)
    for match in matches:
        link = match  # .groups(1)[0]
        tpl_len = len(match)  # .groups(1))
        joined_url = urllib.parse.urljoin(url, link)
        yield from scrape_directory(joined_url, check_for_folder_cached, notify_new_folder,
                                    percentage_begin=percentage_local_begin, percentage_amount=percentage_local_amount)
        percentage_local_begin += percentage_local_amount


def parse_final_directory(url, content, percentage_begin, percentage_amount):
    matches = re.findall(r"href='([\w-]+?.html)'", content)
    percentage_local_begin = percentage_begin
    percentage_local_amount = percentage_amount / len(matches)
    for match in matches:
        link: str = match  # .groups(1)[0]
        if link.lower() != "index.html":
            joined_url = urllib.parse.urljoin(url, link)
            percentage_local_begin += percentage_local_amount
            yield from scrape_email(joined_url, percentage_local_begin)


def extract_header(content, prefix):
    match = re.search(prefix + r":</td>(.*?)</tr>", content, flags=re.DOTALL)
    header_content = match.groups(1)[0]
    return header_content


def extract_emails(content):
    # emails = re.findall(r"<a.*?>(?P<oki>.*?)</a>", content)
    emails = re.findall(r">([^@>]+?@[^@]+?\.[^@]+?)<", content)
    return emails


def extract_date(content):
    datetime_str = re.search(r">(.*?)</td>", content).groups(1)[0]
    # datetime_obj = dateutil.parser.parse(datetime_str)
    return datetime_str


def read_url(url):
    try:
        print(f"  Loading {url}")
        open_url = urllib.request.urlopen(url)
        print(f"  Opened {url}")
        return open_url.read()
    except Exception as e:
        print(url)
        print(e)
        raise e


def scrape_email(email_url, percentage_complete):
    skip = False
    print(f"  Scraping {email_url}")
    try:
        content = read_url(email_url).decode('utf-8')
    except UnicodeDecodeError as e:
        content = read_url(email_url).decode('cp1252')
    except urllib.request.HTTPError as e:
        if e.code in [403, 404]:
            skip = True
            print(e)
        else:
            print(e)
    except Exception as e:
        print(e)
    finally:
        print("  Scraped successfully")
        if not skip:
            email_meta_info = re.search(
                r"<table class.*?>(.*?)</table", content, flags=re.DOTALL).groups(1)[0]
            print("  Regex successful")

            def get_emails(prefix):
                return extract_emails(extract_header(email_meta_info, prefix))

            emails_from = get_emails("From")
            emails_to = get_emails("To")
            emails_cc = get_emails("Cc")
            emails_bcc = get_emails("Bcc")
            date = extract_date(extract_header(email_meta_info, "Date"))
            information_tuple = (emails_from, emails_to, emails_cc,
                                 emails_bcc, date, email_url)
            yield (information_tuple, percentage_complete)


def scrape_directory(url, check_for_folder_cached, notify_new_folder, percentage_begin=0.0, percentage_amount=1.0):
    if check_for_folder_cached(url):
        print(f"Skipping {url}")
    else:
        content = read_url(url).decode('utf-8')
        if re.search(r"<title>Index of ", content):
            yield from parse_directory(url, content, check_for_folder_cached, notify_new_folder, percentage_begin,
                                       percentage_amount)
        elif re.search(r"<title>[\w-]+? folder</title>", content):
            yield from parse_final_directory(url, content, percentage_begin, percentage_amount)
        else:
            raise Exception(f"Found an incompatible folder: {url}")
        notify_new_folder(url)


if __name__ == "__main__":
    already_processed_urls = set()
    skip_file = "skip.txt"
    database = "db.txt"
    estimated_number_of_messages = 600_000
    already_analyzed_messages_count = 0

    if os.path.exists(database):
        with open(database, "r") as db:
            for line in db.readlines():
                already_analyzed_messages_count += 1

    if os.path.exists(skip_file):
        with open(skip_file, "r") as file:
            for line in file.read().splitlines():
                already_processed_urls.add(line)


    def check_for_folder_cached(folder):
        return folder in already_processed_urls


    jsons_to_dump = []


    def notify_new_folder(folder):
        print(f"Dumping {len(jsons_to_dump)} jsons")

        with open(database, "a") as db:
            for json_info in jsons_to_dump:
                db.write(f"{json_info}\n")
        jsons_to_dump.clear()

        print("Jsons dumped")

        with open(skip_file, "a") as file:
            file.write(f"{folder}\n")

        print("Cached new folder: ", folder)


    for (info_tuple, _) in scrape_directory("http://www.enron-mail.com/email/",
                                            check_for_folder_cached, notify_new_folder):
        already_analyzed_messages_count += 1
        shortened_tuple = list(info_tuple)
        shortened_tuple[-1] = shortened_tuple[-1].split(
            "http://www.enron-mail.com/email")[1]
        jsons_to_dump.append(json.dumps(shortened_tuple))
        rough_message_estimate = already_analyzed_messages_count / \
                                 estimated_number_of_messages
        print(f"  Scraped {already_analyzed_messages_count} messages.",
              f"Email from {info_tuple[0]} to {info_tuple[1]}. url: {info_tuple[5]}")
