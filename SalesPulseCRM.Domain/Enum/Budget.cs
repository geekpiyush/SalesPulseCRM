namespace SalesPulseCRM.Domain.Enum
{
    public enum Budget
    {
        Below40L = 1,
        From40LTo60L = 2,
        From60LTo80L = 3,
        From80LTo1Cr = 4,
        From1CrTo2Cr = 5,
        Above2Cr = 6
    }

    public static class BudgetExtensions
    {
        public static string ToDisplay(this Budget budget)
        {
            return budget switch
            {
                Budget.Below40L => "Below 40L",
                Budget.From40LTo60L => "40L - 60L",
                Budget.From60LTo80L => "60L - 80L",
                Budget.From80LTo1Cr => "80L - 1Cr",
                Budget.From1CrTo2Cr => "1Cr - 2Cr",
                Budget.Above2Cr => "Above 2Cr",
                _ => ""
            };
        }
    }
}