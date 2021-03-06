# EDI.Net
EDI Parser/Deserializer. 

This is a ground up implementation and does not make use of `XML Serialization` in any step of the process. This reduces the overhead of converting into multiple formats allong whe way of getting the desired Clr object.

At the moment working for __[Tradacoms](https://en.wikipedia.org/wiki/TRADACOMS)__/__[EDIFact](https://en.wikipedia.org/wiki/EDIFACT)__ formats. 

Using attributes you can express all EDI rules like Mandatory/Conditional Segments,Elements & Components 
as well as describe component values size length and precision with the picture syntax (e.g `9(3)`, `9(10)V9(2)` and `X(3)`). 

#### Installation

To install Edi.Net, run the following command in the Package Manager Console. Or download it [here](https://www.nuget.org/packages/indice.Edi/)

```powershell
PM> Install-Package "indice.Edi"
```

#### Example Usage:
```csharp
var grammar = EdiGrammar.NewTradacoms();
var interchange = default(Interchange);
using (var stream = new StreamReader(@"c:\temp\sample.edi")) {
    interchange = new EdiSerializer().Deserialize<Interchange>(stream, grammar);
}
```
Annotated POCOS example using **part** of Tradacoms UtilityBill format:

```csharp
public class Interchange
{
    [EdiValue("X(14)", Path = "STX/1/0")]
    public string SenderCode { get; set; }

    [EdiValue("X(35)", Path = "STX/1/1")]
    public string SenderName { get; set; }

    [EdiValue("9(6)", Path = "STX/3/0", Format = "yyMMdd", Description = "TRDT - Date")]
    [EdiValue("9(6)", Path = "STX/3/1", Format = "HHmmss", Description = "TRDT - Time")]
    public DateTime TransmissionStamp { get; set; }

    public InterchangeHeader Header { get; set; }

    public InterchangeTrailer Trailer { get; set; }

    public List<UtilityBill> Invoices { get; set; }
}

[EdiMessage, EdiCondition("UTLHDR", Path = "MHD/1")]
public class InterchangeHeader
{
    [EdiValue("9(4)"), EdiPath("TYP")]
    public string TransactionCode { get; set; }

    [EdiValue("9(1)", Path = "MHD/1/1")]
    public int Version { get; set; }
}

[EdiMessage, EdiCondition("UTLTLR", Path = "MHD/1")]
public class InterchangeTrailer
{

    [EdiValue("9(1)", Path = "MHD/1/1")]
    public int Version { get; set; }
}

[EdiMessage, EdiCondition("UTLBIL", Path = "MHD/1")]
public class UtilityBill
{
    [EdiValue("9(1)", Path = "MHD/1/1")]
    public int Version { get; set; }

    [EdiValue("X(17)", Path = "BCD/2/0", Description = "INVN - Date")]
    public string InvoiceNumber { get; set; }

    public MetetAdminNumber Meter { get; set; }
    public ContractData SupplyContract { get; set; }

    [EdiValue("X(3)", Path = "BCD/5/0", Description = "BTCD - Date")]
    public BillTypeCode BillTypeCode { get; set; }

    [EdiValue("9(6)", Path = "BCD/1/0", Format = "yyMMdd", Description = "TXDT - Date")]
    public DateTime IssueDate { get; set; }

    [EdiValue("9(6)", Path = "BCD/7/0", Format = "yyMMdd", Description = "SUMO - Date")]
    public DateTime StartDate { get; set; }

    [EdiValue("9(6)", Path = "BCD/7/1", Format = "yyMMdd", Description = "SUMO - Date")]
    public DateTime EndDate { get; set; }

    public UtilityBillTrailer Totals { get; set; }
    public UtilityBillValueAddedTax Vat { get; set; }
    public List<UtilityBillCharge> Charges { get; set; }

    public override string ToString() {
        return string.Format("{0} TD:{1:d} F:{2:d} T:{3:d} Type:{4}", InvoiceNumber, IssueDate, StartDate, EndDate, BillTypeCode);
    }
}

[EdiSegment, EdiPath("CCD")]
public class UtilityBillCharge
{
    [EdiValue("9(10)", Path = "CCD/0")]
    public int SequenceNumber { get; set; }

    [EdiValue("X(3)", Path = "CCD/1")]
    public ChargeIndicator? ChargeIndicator { get; set; }

    [EdiValue("9(13)", Path = "CCD/1/1")]
    public int? ArticleNumber { get; set; }

    [EdiValue("X(3)", Path = "CCD/1/2")]
    public string SupplierCode { get; set; }

    [EdiValue("9(10)V9(3)", Path = "CCD/10/0", Description = "CONS")]
    public decimal? UnitsConsumedBilling { get; set; }
}
```

#### The Picture clause
The _Picture Clause_ is taken from COBOL laguage and the way it handles expressing numeric and alphanumric data types. It is used throug out tradacoms.

|Symbol | Description      | Example Picture  | Component           | c# result
|:-----:|------------------|:----------------:|---------------------|------------------|
|   9   | Numeric          | `9(3)`          | `013`               | `int v = 13;`
|   A   | Alphabetic       | not used         | -                   | -
|   X   | Alphanumeric     | `X(20)`         | `This is alphanumeric`| `string v = "This is alphanumeric";`
|   V   | Implicit Decimal | `9(5)V(2)`      | `01342`            | `decimal v = 13.42M;`
|   S   | Sign             | not used         | - | - |
|   P   | Assumed Decimal  | not used         | - | - |

#### Roadmap (TODO)

1. Implement serializer `Serialize` to write Clr classes to edi format (Using attributes). (planned for v1.1)
1. Start github wiki page and begin documentation.
1. Consider adding support for X12 format.

_Disclaimer. The project was inspired and influenced by the work done in the excellent library [JSON.Net](https://github.com/JamesNK/Newtonsoft.Json) by James Newton King. Some utility parts for reflection string parsing etc. are used as is_