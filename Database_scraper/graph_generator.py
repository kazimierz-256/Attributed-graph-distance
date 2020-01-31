import networkx as nx
import os
import json
from datetime import datetime
import plotly.graph_objects as go
import itertools
import re


def entries_iterator():
    database_read_filename = "db_cleansed.txt"
    emails = set()
    with open(database_read_filename, "r") as db:
        while (line := db.readline()) != "":
            entry = json.loads(line)
            for email in itertools.chain(entry[0], entry[1], entry[2], entry[3]):
                if re.search(r"@enron\.com", email):
                    emails.add(email)
    pass
            # yield {
            #     "from": entry[0],
            #     "to": entry[1],
            #     "cc": entry[2],
            #     "bcc": entry[3],
            #     "date": datetime.strptime(entry[4]),
            #     "url": entry[6]
            # }


if __name__ == "__main__":
    def custom_filter(entry):
        time_from = datetime(2000, 1, 1)
        time_to = datetime(2010, 1, 1)
        return time_to > entry["date"] > time_from
    entries = entries_iterator()
    matching_entries = filter(custom_filter, entries)
    for entry in matching_entries:
        print(entry["date"])

    # fig = go.Figure(
    #     data=[go.Bar(y=[2, 1, 3])],
    #     layout_title_text="A Figure Displayed with fig.show()"
    # )
    # fig.show()
