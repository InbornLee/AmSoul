using AmSoul.Core;
using SqlSugar;

namespace Sample.Models;
[SugarTable("INPATIENT_COST_CLASSFICATION")]
public class OperationCost : IDataModel
{

    [SugarColumn(IsPrimaryKey = true)]
    public string? Id { get; set; }
    public string? SecondCategoryCode { get; set; }
    public string? SecondCategoryName { get; set; }
    public string? CostItemCode { get; set; }
    public string? ItemName { get; set; }
    public double? CostMoney { get; set; }
}
