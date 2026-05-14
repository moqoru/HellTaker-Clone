using System.Collections.Generic;
using System.Text;

/// <summary>
/// CSV 텍스트 파싱을 위한 순수 함수 유틸리티.
/// 따옴표 안의 콤마 처리, 3중 따옴표/이스케이프 정규화 등을 담당.
/// 단위 테스트 가능한 정적 헬퍼.
/// </summary>
public static class CsvUtility
{
    /// <summary>
    /// CSV 한 줄을 셀 단위로 분리.
    /// 따옴표 안의 콤마는 셀 구분자로 취급하지 않음.
    /// </summary>
    public static string[] ParseLine(string line)
    {
        if (line == null)
        {
            return new string[0];
        }

        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    /// <summary>
    /// CSV 셀 값을 정규화.
    /// - 앞뒤 따옴표 제거 (3중 따옴표 포함)
    /// - 이스케이프된 따옴표("") → 단일 따옴표(")
    /// - \n 문자열 → 실제 줄바꿈
    /// - 앞뒤 공백 제거
    /// </summary>
    public static string CleanString(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";

        str = str.Trim();
        if (str.StartsWith("\"\"\"")) str = str.Substring(3);
        if (str.EndsWith("\"\"\"")) str = str.Substring(0, str.Length - 3);
        if (str.StartsWith("\"")) str = str.Substring(1);
        if (str.EndsWith("\"")) str = str.Substring(0, str.Length - 1);

        str = str.Replace("\"\"", "\"");
        str = str.Replace("\\n", "\n");

        return str.Trim();
    }
}