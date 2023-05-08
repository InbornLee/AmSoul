using AmSoul.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sample.Models;

/// <summary>
/// 核酸打印记录
/// </summary>
public class PrintRecord : IDataModel
{
    /// <summary>
    /// ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    /// <summary>
    /// 患者姓名
    /// </summary>
    public string? PatientName { get; set; }
    /// <summary>
    /// 性别
    /// </summary>
    public int GenderCode { get; set; }
    /// <summary>
    /// 就诊号
    /// </summary>
    public string? RegisterCode { get; set; }
    /// <summary>
    /// 年龄
    /// </summary>
    public string? Age { get; set; }
    /// <summary>
    /// 条码号
    /// </summary>
    public string? BarCode { get; set; }
    /// <summary>
    /// 交易时间
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime ChargeTime { get; set; }
    /// <summary>
    /// 交易状态
    /// </summary>
    public string? ChargeTstatus { get; set; }
    /// <summary>
    /// 交易来源
    /// </summary>
    public string? ChargeUser { get; set; }
    /// <summary>
    /// 费用编码
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// 项目数量
    /// </summary>
    public double CostCount { get; set; }
    /// <summary>
    /// 项目单价
    /// </summary>
    public double CostMoney { get; set; }
    /// <summary>
    /// 项目名称
    /// </summary>
    public string? CostName { get; set; }
    /// <summary>
    /// 缴费金额
    /// </summary>
    public double CostPrice { get; set; }
    /// <summary>
    /// 支付方式
    /// </summary>
    public string? PayType { get; set; }
    /// <summary>
    /// 订单号
    /// </summary>
    public string? SettlementCode { get; set; }
    /// <summary>
    /// 打印时间
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime PrintTime { get; set; }
    /// <summary>
    /// 打印人
    /// </summary>
    public string? PrintOperator { get; set; }
}
