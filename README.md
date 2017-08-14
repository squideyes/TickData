The HistDataFetch utility downloads ASCII tick data from HistData.com then converts the data into both CSV and fixed-length "TICKS" format files.  NOTE: All times are adjusted to EST and honor Daylight Savings Time.

Before running HistDataFetch.exe, you need to set a few configuration items in the "HistdataFetch.exe.config" file to appropriate values.

| Value | Notes |
| ----- | ----- |
| TickDataPath | The folder to save tick-data to (i.e. C:\TickData).  If the folder doesn't exist it will be created. |
| FirstYearToFetch | The first year to fetch tick-data for.  The year must be >= 2010.  Also, there is no "LastYearToFetch;" tick-data will be fetched up until the most recently available full month. |
| AssetsToFetch | A comma-separated list of one or more  assets to fetch tick-data for (can be any combination of AUDCAD, AUDCHF, AUDHKD, AUDJPY, AUDNZD, AUDSGD, AUDUSD, CADCHF, CADHKD, CADJPY, CADSGD, CHFHKD, CHFJPY, CHFZAR, EURAUD, EURCAD, EURCHF, EURCZK, EURDKK, EURGBP, EURHKD, EURHUF, EURJPY, EURNOK, EURNZD, EURPLN, EURSEK, EURSGD, EURTRY, EURUSD, EURZAR, GBPAUD, GBPCAD, GBPCHF, GBPHKD, GBPJPY, GBPNZD, GBPPLN, GBPSGD, GBPUSD, GBPZAR, HKDJPY, NZDCAD, NZDCHF, NZDHKD, NZDJPY, NZDSGD, NZDUSD, SGDCHF, SGDHKD, SGDJPY, TRYJPY, USDCAD, USDCHF, USDCNH, USDCZK, USDDKK, USDHKD, USDHUF, USDINR, USDJPY, USDMXN, USDNOK, USDPLN, USDSAR, USDSEK, USDSGD, USDTHB, USDTRY, USDZAR, ZARJPY). 

Each time the program runs it will save status and error info to a dated log file in the "Logs" sub-folder of the HistDataFetch.exe installation folder.

**CSV FORMAT**

Each CSV file contains 24 hours of tick (bid/ask) data in "**{Symbol},{TickOn},{BidRate},{AskRate}**" format, beginning with the 5:00pm EST market open.  The files are named "**{Source}\_{Symbol}\_{YYYYMMDD}\_17\_24\_EST.csv**" YYYYMMDD is the date associated with the earliest possible tick in the file.

| Field | DataKind | Notes |
| ------ | ------ | ------ |
| Symbol | string | The symbol associated with the bid/ask (i.e. EURUSD). |
| TickOn | long | The moment associated with the bid/ask; in YYYYMMDD HH:MM:SS.FFF.   |
| BidRate | double | A "bid" rate, formatted to match it's related Symbol (i.e. a EURUSD bid would have 5 digits whereas a USDJPY bid would only have 3).  |
| AskRate | double | An "ask" rate, formatted to match it's related Symbol (i.e. a EURUSD ask would have 5 digits whereas a USDJPY ask  would only have 3).  |


**TICKS FORMAT**

Each TICKS file contains 24 hours of tick (bid/ask) data in a compressed fixed-field format, beginning with the 5:00pm EST market open.  The files are named   "**{Source}\_{Symbol}\_{YYYYMMDD}\_17\_24\_EST.ticks**".  YYYYMMDD is the date associated with the earliest possible tick in the file.  The file starts off with a single HEADER block followed by zero or more TICK-DATA blocks.  The data is  Little-Endian and zipped.

HEADER

| Field | DataKind | Notes |
| ------ | ------ | ------ |
| Version | float | The version number associated with this data layout. | 
| CreatedOn | long | The moment the file was created; in TimeSpan.Ticks since DateTime.MinValue. |
| Source | char(8) | The source/provider of the tick data (i.e. HISTDATA). |
| Symbol | char(8) | The symbol associated with the tick data (i.e. EURUSD). |
| BaseDate | long | The date associated with the earliest possible tick in the file; in TimeSpan.Ticks since DateTime.MinValue. |
| Spacer | byte[24] | A spacer to help with data alignment. | 
| Count | int | The number of TICK-DATA blocks contained within the file. |

TICK-DATA BLOCK

| Field | DataKind | Notes |
| ------ | ------ | ------ |
| TickOn | long | The moment associated with the bid/ask; in YYYYMMDD HH:MM:SS.FFF.   |
| BidRate | double | A "bid" rate, formatted to match it's related Symbol (i.e. a EURUSD bid would have 5 digits whereas a USDJPY bid would only have 3).  |
| AskRate | double | An "ask" rate, formatted to match it's related Symbol (i.e. a EURUSD ask would have 5 digits whereas a USDJPY ask  would only have 3).  |



