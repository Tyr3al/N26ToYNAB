# N26ToYNAB

This repository contains a simple CLI tool to convert N26 CSV files to YNAB compatible CSVs.

## Installation

Download the code and compile it with .NET5. You can also download the binary from the release section.
To use the binary, add the it to your PATH Environment variable or switch to the containing folder with your shell / terminal. Now you can use the commands below.

## Usage

To use the tool, you only need to specify the path to the downloaded N26 CSV file. Optionally you can specify the path for the output file that can be uploaded to YNAB.

If you do not specify the output file path, the tool will create the CSV next to the input file, e.g. `C:\n26.csv` -> `C:\n26_ynab.csv`

```bash
# Create ynab file without specifying output file
N26ToYNAB -i "C:\Users\Foo\Desktop\n26-csv-transactions.csv"

# Create ynab file by specifying output file
N26ToYNAB -i "C:\Users\Foo\Desktop\n26-csv-transactions.csv" -o "C:\Users\Foo\Desktop\ynab-csv-transactions.csv"
```


```text
N26ToYNAB 0.1.0
Copyright (C) 2021 Jan Funke - Tyr3al

  -i, --input     Required. Path to N26 CSV file

  -o, --output    Path to generated YNAB csv (optional)

  --help          Display this help screen.

  --version       Display version information.
```
