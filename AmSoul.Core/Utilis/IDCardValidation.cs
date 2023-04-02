using System.Collections;

namespace AmSoul.Core.Utilis;

/// <summary>
/// 身份证号验证工具
/// </summary>
public class IDCardValidation
{
    //省、直辖市代码
    public static Hashtable cityCodes = new()
    {
        { "11", "北京" },
        { "12", "天津" },
        { "13", "河北" },
        { "14", "山西" },
        { "15", "内蒙古" },
        { "21", "辽宁" },
        { "22", "吉林" },
        { "23", "黑龙江" },
        { "31", "上海" },
        { "32", "江苏" },
        { "33", "浙江" },
        { "34", "安徽" },
        { "35", "福建" },
        { "36", "江西" },
        { "37", "山东" },
        { "41", "河南" },
        { "42", "湖北" },
        { "43", "湖南" },
        { "44", "广东" },
        { "45", "广西" },
        { "46", "海南" },
        { "50", "重庆" },
        { "51", "四川" },
        { "52", "贵州" },
        { "53", "云南" },
        { "54", "西藏" },
        { "61", "陕西" },
        { "62", "甘肃" },
        { "63", "青海" },
        { "64", "宁夏" },
        { "65", "新疆" },
        { "71", "台湾" },
        { "81", "香港" },
        { "82", "澳门" },
        { "91", "国外" }
    };
    //校对系数常数
    private static readonly int[] Wi = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
    //校验码常数
    private static readonly string[] VarifyCode = new string[] { "1", "0", "X", "9", "8", "7", "6", "5", "4", "3", "2" };
    static IDCardValidation()
    {

    }
    //获取身份证性别
    public static string GetIDCardGender(string? idNumber)
    {
        if (string.IsNullOrEmpty(idNumber)) throw new Exception("身份证号码为空");
        if (idNumber.Length == 15) idNumber = Conver15To18(idNumber);
        _ = int.TryParse(idNumber.Substring(16, 1), out int g);
        if (g % 2 == 0)
            return "女";
        else return "男";
    }
    public static string GetIdCardAge(string? idNumber)
    {
        if (string.IsNullOrEmpty(idNumber)) throw new Exception("身份证号码为空");
        if (idNumber.Length != 18)
            if (idNumber.Length != 15)
                throw new Exception("身份证号码只能为15位或18位");
            else
                idNumber = Conver15To18(idNumber);//如果为15位转换至18位
        string b = idNumber.Substring(6, 4) + "-" + idNumber.Substring(10, 2) + "-" + idNumber.Substring(12, 2);
        return GetAgeByBirthday(DateTime.Parse(b));
    }
    /// <summary>
    /// 校验身份证合理性
    /// </summary>
    /// <param name="idNumber"></param>
    /// <returns></returns>
    public static bool CheckIDCard(string idNumber)
    {
        var l = idNumber.Length;
        return l switch
        {
            18 => CheckIDCard18(idNumber),
            15 => CheckIDCard15(idNumber),
            _ => false,
        };
    }

    /// <summary>
    /// 15位转换为18位
    /// </summary>
    /// <param name="perID15">15位身份证号码</param>
    /// <returns></returns>
    private static string Conver15To18(string perID15)
    {
        //新身份证号
        if (perID15.Length != 15) throw new Exception("身份证号码位数错误(应为15位)");

        var perID18 = perID15.Substring(0, 6);
        //填在第6位及第7位上填上‘1’，‘9’
        perID18 += "19";
        //填入生日
        perID18 += perID15.Substring(6, 9);
        //进行加权求和
        int iS = 0;
        for (int i = 0; i < 17; i++)
        {
            iS += int.Parse(perID18.Substring(i, 1)) * Wi[i];
        }
        //取模运算
        int iY = iS % 11;
        //从VarifyCode中取得以模为索引号的值，加到身份证的最后一位，即为新身份证号。
        perID18 += VarifyCode[iY];
        return perID18;
    }
    /// <summary>  
    /// 18位身份证号码验证  
    /// </summary>  
    private static bool CheckIDCard18(string idNumber)
    {

        if (long.TryParse(idNumber.Remove(17), out long n) == false
            || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
        {
            return false;//数字验证  
        }
        string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        if (!address.Contains(idNumber.Remove(2), StringComparison.CurrentCulture))
        {
            return false;//省份验证  
        }
        string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
        _ = new DateTime();
        if (DateTime.TryParse(birth, out _) == false)
        {
            return false;//生日验证  
        }
        string[] arrVarifyCode = "1,0,x,9,8,7,6,5,4,3,2".Split(',');
        string[] Wi = "7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2".Split(',');
        char[] Ai = idNumber.Remove(17).ToCharArray();
        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
        }
        Math.DivRem(sum, 11, out int y);
        if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
        {
            return false;//校验码验证  
        }
        return true;//符合GB11643-1999标准  
    }


    /// <summary>  
    /// 15位身份证号码验证  
    /// </summary>  
    private static bool CheckIDCard15(string idNumber)
    {
        if (long.TryParse(idNumber, out long n) == false || n < Math.Pow(10, 14))
            return false;//数字验证  
        string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        if (address.IndexOf(idNumber.Remove(2)) == -1)
            return false;//省份验证  
        string birth = idNumber.Substring(6, 6).Insert(4, "-").Insert(2, "-");
        if (DateTime.TryParse(birth, out _) == false)
            return false;//生日验证  
        return true;
    }
    /// <summary>
    /// 获取出生日期
    /// </summary>
    /// <param name="perID18"></param>
    /// <returns></returns>
    public static DateTime? GetBirthday(string perID18)
    {
        string birthday = perID18.Substring(6, 8).Insert(6, "-").Insert(4, "-");
        if (DateTime.TryParse(birthday, out DateTime result) == false)
        {
            return DateTime.MinValue;//生日验证  
        }
        return result;
    }

    /// <summary>
    /// 根据出生年月计算 X岁或X月X天或X天
    /// </summary>
    /// <param name="birthday"></param>
    /// <returns></returns>
    private static string GetAgeByBirthday(DateTime birthday)
    {
        DateTime currenttime = DateTime.Now;
        TimeSpan diffTime = currenttime - birthday;
        if (diffTime.TotalDays < 365)
        {
            //个月计算
            int diffmonth = currenttime.Month - birthday.Month;
            int day = currenttime.Day - birthday.Day;
            if (day < 0)
            {
                var v = diffmonth--;
                _ = v;
            }
            if (diffmonth <= 0)
            {
                //直接计算天
                return $"{(int)diffTime.TotalDays}天";
            }
            else
            {
                DateTime newbirthday = birthday.AddMonths(diffmonth);
                day = (int)(currenttime - newbirthday).TotalDays;
                return $"{diffmonth}个月{(day == 0 ? "" : day.ToString() + "天")}";
            }
        }
        else
        {
            //年龄计算
            return $"{currenttime.Year - birthday.Year}岁";
        }

    }
}
