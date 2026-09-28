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

public enum Brand
{
    Jotun = 0,
    International = 1
}

public enum ProductType
{
    Coating = 0,
    Base = 1,
    CuringAgent = 2,
    Thinner = 3,
    Cleaner = 4
}

public enum GlossLevel
{
    Matt = 0,
    Eggshell = 1,
    SemiGloss = 2,
    Gloss = 3,
    FullGloss = 4
}