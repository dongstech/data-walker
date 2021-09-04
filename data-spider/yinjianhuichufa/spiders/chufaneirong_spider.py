import scrapy
import json
import math
import re
from datetime import datetime
from docparser import docparser

class ChufaneirongSpider(scrapy.Spider):
    name = "chufaneirong"
    pageSize = 18
  
    def start_requests(self):
        urls = [
            'https://www.cbirc.gov.cn/cn/static/data/DocInfo/SelectDocByItemIdAndChild/data_itemId=4113,pageIndex=1,pageSize=18.json',
            'https://www.cbirc.gov.cn/cn/static/data/DocInfo/SelectDocByItemIdAndChild/data_itemId=4114,pageIndex=1,pageSize=18.json',
            'https://www.cbirc.gov.cn/cn/static/data/DocInfo/SelectDocByItemIdAndChild/data_itemId=4115,pageIndex=1,pageSize=18.json'
        ]
        for url in urls:
            yield scrapy.Request(url=url, callback=self.parse)

    def parse(self, response):
        # self.log(f'[parse]{response.url}')
        self.log(f'Year: {self.year}')
        url = response.url
        match = re.search(
            r"^https.*data_itemId=(?P<data_item_id>\d{4}),pageIndex", url)
        data_item_id = match.group('data_item_id')
        respObj = json.loads(response.text)
        if respObj['rptCode'] == 200:
            pageSize = self.pageSize
            totalDocs = respObj['data']['total']
            totalPages = math.ceil(totalDocs/pageSize)
            for pageIndex in range(totalPages):
                pageIndex += 1
                pageUri = f'https://www.cbirc.gov.cn/cbircweb/DocInfo/SelectDocByItemIdAndChild?itemId={data_item_id}&pageIndex={pageIndex}&pageSize={pageSize}'
                yield scrapy.Request(url=pageUri, callback=self.parse_doc_list)

    def parse_doc_list(self, response):
        respObj = json.loads(response.text)
        if respObj['rptCode'] == 200:
            docs = respObj['data']['rows']

            for doc in docs:
                docId = doc['docId']
                publishDate = datetime.strptime(
                    doc['publishDate'], '%Y-%m-%d %H:%M:%S')  # 2005-07-09 10:08:00
                docUri = f'https://www.cbirc.gov.cn/cn/static/data/DocInfo/SelectByDocId/data_docId={docId}.json'
                if publishDate.year == int(self.year):
                    parser = docparser.DocParser()
                    yield scrapy.Request(url=docUri, callback=parser.parse_doc)