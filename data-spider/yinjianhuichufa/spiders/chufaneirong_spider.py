import scrapy
import json
import math
import re
from datetime import datetime
from scrapy import Selector


class ChufaneirongSpider(scrapy.Spider):
    name = "chufaneirong"
    pageSize = 18
    fields = {
        'field1': '行政处罚决定书文号',
        'field2': '被处罚当事人',
        'field3': '主要违法违规事实（案由）',
        'field4': '行政处罚依据',
        'field5': '行政处罚决定',
        'field6': '作出处罚决定的机关名称',
        'field7': '作出处罚决定的日期',
        'field8': 'origin_content'
    }
    year = 2015

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
        url = response.url
        match = re.search(r"^https.*data_itemId=(?P<data_item_id>\d{4}),pageIndex",url)
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
            yearWindow = 3
            
            for doc in docs:
                docId = doc['docId']
                publishDate = datetime.strptime(doc['publishDate'],'%Y-%m-%d %H:%M:%S') #2005-07-09 10:08:00
                docUri = f'https://www.cbirc.gov.cn/cn/static/data/DocInfo/SelectByDocId/data_docId={docId}.json'
                if datetime.today().year - publishDate.year < yearWindow :
                    yield scrapy.Request(url=docUri, callback=self.parse_doc)

    def parse_doc(self, response):
        respObj = json.loads(response.body)
        if respObj['rptCode'] == 200:
            html = respObj['data']['docClob']
            selector = Selector(text=html)
            result = {}
            doc_id = respObj['data']['docId']
            result['link'] = f'https://www.cbirc.gov.cn/cn/view/pages/ItemDetail.html?docId={doc_id}&itemId=4113&generaltype=9'

            table = selector.xpath('//div[@class="Section0"]//table | //div[@class="Section1"]//table')

            if len(table) > 0:
                row_number = len(table.xpath('./tr'))
                if row_number == 7:
                    for i in range(row_number):
                        rowIdx = i+1
                        value = table.xpath(
                            f'./tr[{rowIdx}]/td[2]/p//text()').getall()
                        value = ''.join(value)
                        result[self.fields[f'field{rowIdx}']] = value
                    result[self.fields['field8']] = ''
                    yield result
                elif row_number == 8:
                    for i in range(row_number):
                        row_idx = i + 1
                        result[self.fields[f'field{row_idx}']] = ''
                    result[self.fields['field8']] = '8'
                elif row_number == 9:
                    for i in range(row_number):
                        row_idx = i + 1
                        result[self.fields[f'field{row_idx}']] = ''
                    result[self.fields['field8']] = '9'
                else:
                    for i in range(row_number):
                        row_idx = i + 1
                        result[self.fields[f'field{row_idx}']] = ''
                    result[self.fields['field8']] = row_number
                yield result
            else:
                for i in range(len(self.fields)):
                    row_idx = i+1
                    result[self.fields[f'field{row_idx}']] = ''
                origin_content = selector.xpath('//p//text()').getall()
                origin_content = ''.join(origin_content)
                result[self.fields['field8']] = origin_content
                yield result
