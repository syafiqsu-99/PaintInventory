namespace PaintInventory.Server.Models;

public enum ComponentType
{
    Single = 0,
    PartA = 1,
    PartB = 2
}

public enum StockDirection
{
    In = 0,
    Out = 1,
    Adjustment = 2,
    Transfer = 3
}

public enum CoatType
{
    Primer = 1,
    SecondCoat = 2,
    ThirdCoat = 3,
    FourthCoat = 4
}

public enum AdhesionTestType
{
    None = 0,
    TestPlate = 1,
    ProductionPart = 2
}