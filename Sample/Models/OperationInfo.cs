using AmSoul.Core;
using SqlSugar;

namespace Sample.Models;
[SugarTable("operationinfos")]
public class OperationInfo : IDataModel
{
    /// <summary>
    /// 住院号
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public string? Id { get; set; }
    /// <summary>
    /// 手术ID
    /// </summary>
    public string? RegOptId { get; set; }
    /// <summary>
    /// 患者姓名
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 患者性别
    /// </summary>
    public string? Sex { get; set; }
    /// <summary>
    /// 生日
    /// </summary>
    public string? Birthday { get; set; }
    /// <summary>
    /// 年龄
    /// </summary>
    public int? Age { get; set; }
    /// <summary>
    /// 床号
    /// </summary>
    public string? Bed { get; set; }
    /// <summary>
    /// 科室ID
    /// </summary>
    public string? DeptId { get; set; }
    /// <summary>
    /// 科室名称
    /// </summary>
    public string? DeptName { get; set; }
    /// <summary>
    /// 手术日期
    /// </summary>
    public string? OperaDate { get; set; }
    /// <summary>
    /// 拟施手术名称
    /// </summary>
    public string? DesignedOptName { get; set; }
    /// <summary>
    /// 拟施手术代码
    /// </summary>
    public string? DesignedOptCode { get; set; }
    /// <summary>
    /// 手术级别
    /// </summary>
    public string? OptLevel { get; set; }
    /// <summary>
    /// 是否急诊？日间？
    /// </summary>
    public int? Emergency { get; set; }
    /// <summary>
    /// 术者ID（工号）
    /// </summary>
    public string? OperatorId { get; set; }
    /// <summary>
    /// 术者名称
    /// </summary>
    public string? OperatorName { get; set; }
    /// <summary>
    /// 助手ID（工号）
    /// </summary>
    public string? AssistantId { get; set; }
    /// <summary>
    /// 助手名称
    /// </summary>
    public string? AssistantName { get; set; }
    /// <summary>
    /// 麻醉记录ID
    /// </summary>
    public string? AnaRecordId { get; set; }
    /// <summary>
    /// 麻醉小结ID
    /// </summary>
    public string? AnaSumId { get; set; }
    /// <summary>
    /// 麻醉小结
    /// </summary>
    public string? AnestSummary { get; set; }
    /// <summary>
    /// 麻醉师ID（工号）
    /// </summary>
    public string? AnesthetistId { get; set; }
    /// <summary>
    /// 麻醉师名称
    /// </summary>
    public string? AnesthetistName { get; set; }
    /// <summary>
    /// ASA级别
    /// </summary>
    public string? AsaLevel { get; set; }
    /// <summary>
    /// 麻醉级别
    /// </summary>
    public string? AnaesLevel { get; set; }
    /// <summary>
    /// 麻醉开始时间
    /// </summary>
    public string? AnaesStartTime { get; set; }
    /// <summary>
    /// 麻醉结束时间
    /// </summary>
    public string? AnaesEndTime { get; set; }
    /// <summary>
    /// 手术开始时间
    /// </summary>
    public string? OperStartTime { get; set; }
    /// <summary>
    /// 手术结束时间
    /// </summary>
    public string? OperEndTime { get; set; }
    /// <summary>
    /// 总手术时间
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public double? OperaTotalMinutes => !string.IsNullOrEmpty(OperStartTime) && !string.IsNullOrEmpty(OperEndTime) ? Math.Ceiling(Convert.ToDateTime(OperEndTime).Subtract(Convert.ToDateTime(OperStartTime)).TotalMinutes) : null;
    /// <summary>
    /// 入室时间
    /// </summary>
    public string? InOperRoomTime { get; set; }
    /// <summary>
    /// 出室时间
    /// </summary>
    public string? OutOperRoomTime { get; set; }
    /// <summary>
    /// 手术间
    /// </summary>
    public string? OperRoomName { get; set; }
    /// <summary>
    /// 去向
    /// </summary>
    public string? LeaveTo { get; set; }
}
