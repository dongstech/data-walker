import scrapy


class JiguanSpider(scrapy.Spider):
    name = "jiguan"

    def start_requests(self):
        urls = [
            'https://www.cbirc.gov.cn/cn/view/pages/ItemList.html?itemPId=923&itemId=4113&itemUrl=ItemListRightList.html&itemName=%E9%93%B6%E4%BF%9D%E7%9B%91%E4%BC%9A%E6%9C%BA%E5%85%B3&itemsubPId=931&itemsubPName=%E8%A1%8C%E6%94%BF%E5%A4%84%E7%BD%9A'
        ]
        https://www.cbirc.gov.cn/cn/static/data/DocInfo/SelectByDocId/data_docId=967722.json
        for url in urls:
            yield scrapy.Request(url=url, callback=self.parse)

    def parse(self, response):
        self.log(f'[parse]{response.url}')
        self.log(f'[html]{response.text}')
        yield from response.follow_all(response.css('div.panel-row span>a'), callback=self.parse_doc)

        next_page = response.css('div.main div.caidan-right-div>div.row>div:nth-last-child(2)>div>a:nth-last-child(2)').get()
        self.log(f'[next page]{next_page}')
    
    def parse_doc(self, response):
        self.log(f'[parse_doc]{response.url}')