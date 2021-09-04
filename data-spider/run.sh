year=$1
scrapy crawl chufaneirong -a year=$year -O result_$year.csv
# scrapy crawl jiguan -O jiguan.pickle