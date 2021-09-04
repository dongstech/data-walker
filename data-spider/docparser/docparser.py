import json
from scrapy import Selector

class DocParser:

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

    def parse_doc(self, response):
        respObj = json.loads(response.body)
        if respObj['rptCode'] == 200:
            html = respObj['data']['docClob']
            selector = Selector(text=html)
            result = {}
            doc_id = respObj['data']['docId']

            result['link'] = f'https://www.cbirc.gov.cn/cn/view/pages/ItemDetail.html?docId={doc_id}&itemId=4113&generaltype=9'

            table = selector.xpath(
                '//div[@class="Section0"]//table | //div[@class="Section1"]//table')

            if len(table) > 0:
                row_number = len(table.xpath('./tr'))
                # self.log(f'parse_doc table {row_number} rows...')
                if row_number == 7:
                    for i in range(row_number):
                        rowIdx = i+1
                        value = table.xpath(
                            f'./tr[{rowIdx}]/td[2]//text()').getall()
                        value = ''.join(value)
                        result[self.fields[f'field{rowIdx}']] = value
                    result[self.fields['field8']] = table.xpath(
                        './/text()').getall()
                elif row_number == 8:
                    result = result | self.parse_table_8_rows(selector)
                elif row_number == 9:
                    # self.log('call parse_table_9_rows...')
                    result = result | self.parse_table_9_rows(selector)
                elif row_number == 10:
                    # self.log('call parse_table_10_rows...')
                    result = result | self.parse_table_10_rows(selector)
                else:
                    for i in range(len(self.fields)):
                        row_idx = i + 1
                        result[self.fields[f'field{row_idx}']] = ''
                    result[self.fields['field8']] = row_number
            else:
                for i in range(len(self.fields)):
                    row_idx = i+1
                    result[self.fields[f'field{row_idx}']] = ''
                origin_content = selector.xpath('///text()').getall()
                origin_content = ''.join(origin_content)
                result[self.fields['field8']] = origin_content
            yield result

    def parse_table_8_rows(self, selector):
        # self.log('parse_table_8_rows...')
        result = {}
        table = selector.xpath(
            '//div[@class="Section0"]//table | //div[@class="Section1"]//table | //div[@class="WordSection1"]//table')
        result[self.fields['field1']] = "".join(table.xpath(
            './tr[1]/td[2]//text()').getall())

        result[self.fields['field2']] = " ".join(table.xpath('./tr[2]/td[3]//text()').getall() + table.xpath(
            './tr[3]/td[2]//text()').getall())

        result[self.fields['field3']] = "".join(table.xpath(
            './tr[5]/td[2]//text()').getall())
        result[self.fields['field4']] = "".join(table.xpath(
            './tr[6]/td[2]//text()').getall())
        result[self.fields['field5']] = "".join(table.xpath(
            './tr[7]/td[2]//text()').getall())
        result[self.fields['field6']] = "".join(table.xpath(
            './tr[8]/td[2]//text()').getall())
        result[self.fields['field7']] = "".join(table.xpath(
            './tr[9]/td[2]//text()').getall())
        result[self.fields['field8']] = "".join(table.xpath(
            './/text()').getall())
        return result

    def parse_table_9_rows(self, selector):
        # self.log('parse_table_9_rows...')
        result = {}
        table = selector.xpath(
            '//div[@class="Section0"]//table | //div[@class="Section1"]//table')
        result[self.fields['field1']] = "".join(table.xpath(
            './tr[1]/td[2]//text()').getall())

        result[self.fields['field2']] = "".join(table.xpath('./tr[2]/td[3]//text()').getall() + table.xpath(
            './tr[3]/td[3]//text()').getall() + table.xpath('./tr[4]/td[2]//text()').getall())

        result[self.fields['field3']] = "".join(table.xpath(
            './tr[5]/td[2]//text()').getall())
        result[self.fields['field4']] = "".join(table.xpath(
            './tr[6]/td[2]//text()').getall())
        result[self.fields['field5']] = "".join(table.xpath(
            './tr[7]/td[2]//text()').getall())
        result[self.fields['field6']] = "".join(table.xpath(
            './tr[8]/td[2]//text()').getall())
        result[self.fields['field7']] = "".join(table.xpath(
            './tr[9]/td[2]//text()').getall())
        result[self.fields['field8']] = "".join(table.xpath(
            './/text()').getall())
        return result

    def parse_table_10_rows(self, selector):
        # self.log('parse_table_10_rows...')
        result = {}
        table = selector.xpath(
            '//div[@class="Section0"]//table | //div[@class="Section1"]//table')
        result[self.fields['field1']] = "".join(table.xpath(
            './tr[1]/td[2]//text()').getall()).strip()

        result[self.fields['field2']] = "".join(table.xpath('./tr[2]/td[4]//text()').getall() + table.xpath(
            './tr[3]/td[2]//text()').getall() + table.xpath('./tr[4]/td[3]//text()').getall() + table.xpath('./tr[5]/td[2]//text()').getall()).strip()

        result[self.fields['field3']] = "".join(table.xpath(
            './tr[6]/td[2]//text()').getall())
        result[self.fields['field4']] = "".join(table.xpath(
            './tr[7]/td[2]//text()').getall())
        result[self.fields['field5']] = "".join(table.xpath(
            './tr[8]/td[2]//text()').getall())
        result[self.fields['field6']] = "".join(table.xpath(
            './tr[9]/td[2]//text()').getall())
        result[self.fields['field7']] = "".join(table.xpath(
            './tr[10]/td[2]//text()').getall())
        result[self.fields['field8']] = "".join(table.xpath(
            './/text()').getall())
        return result