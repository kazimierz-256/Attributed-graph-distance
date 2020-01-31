import json
import re
from itertools import chain
from functools import reduce
import os

_fixing_patterns = [
    (r"</b>", ""),
    (r"</a>", ""),
    (r"\&lt", ""),
    (r"\;", ""),
    (r"\"", ""),
    (r",", ""),
    (r"\.\.", r"\."),
    (r"\s", r""),
    (r"\.$", r""),
    (r"\.@", r"@"),
    (r"@enron$", r"@enron.com"),
    (r"@(\w+)$", r"@\1.com"),
    (r"\'", r""),
]


def fix_email(email):
    if not is_typical_email(email):
        def apply_pattern(element, fixing_tuple):
            pattern, replacement = fixing_tuple
            replaced = re.sub(pattern, replacement, element)
            return replaced
        return reduce(apply_pattern, _fixing_patterns, email)
    else:
        return email

def is_typical_email(email):
    return re.search(r"\w+?([\.-]\w+)*?@([\w-]+?\.)+?[\w-]+?", email) is not None


if __name__ == "__main__":
    database_read_filename = "db_cleansed.txt"
    database_write_filename = "db_clean.txt"
    entries = {}
    read_lines = 0
    wrote_lines = 0
    with open(database_write_filename, "w") as db_w:
        with open(database_read_filename, "r") as db:
            for line in db.readlines():
                read_lines += 1
                entry = json.loads(line)
                # json dumps
                if entries.get(entry[-1], None) is not None:
                    raise Exception(f"Found a duplicate email: {entry[-1]}")
                entries[entry[-1]] = entry
                for i in range(4):
                    entry[i] = [fix_email(email) for email in entry[i]]
                correct_email_addresses = True
                # for email in chain(*entry[:4]):
                #     if not is_typical_email(email):
                #         correct_email_addresses = False
                #         break
                # if correct_email_addresses:
                #     wrote_lines += 1
                db_w.write(f"{json.dumps(entry)}\n")
    print("Completed loading database")
    print(read_lines, wrote_lines)

    # raw_nontypical_emails = chain.from_iterable(() for (k, v) in entries.items())

    # non_typical_emails = dict((email, (original_email, v[-1])) for (email, original_email, v) in raw_nontypical_emails if not is_typical_email(email))

    # for (email, (original_email, url)) in non_typical_emails.items():
    #     print(original_email)
    #     print(email)
    #     # print(f"http://www.enron-mail.com/email{url}")
    #     print()

    # fixed_emails = dict((email, fix_email(email))
    #                     for email, url in non_typical_emails.items())

    # # for email, fixed_email in fixed_emails.items():
    # #     print(f"Fixed '{email}' to '{fixed_email}'")
    # print(f"Messed up email count: {len(non_typical_emails)}")
