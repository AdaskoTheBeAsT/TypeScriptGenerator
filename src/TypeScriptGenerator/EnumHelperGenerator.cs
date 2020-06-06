using System.IO;
using System.Text;

namespace TypeScriptGenerator
{
    public class EnumHelperGenerator
    {
        public void Generate(string targetPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine(CodeGenerator.Header);
            sb.Append(@"
export abstract class EnumHelper {
  static getKeys<T>(type: T): string[] {
    const keys: string[] = [];
    const filteredKeys = Object.keys(type)
      .filter((k) => {
        return !/\d+/.test(k);
      });
    for (let key of filteredKeys) {
      const converted = key as keyof typeof type;
      const val = type[converted];
      if (typeof val === 'function') {
        continue;
      }
      keys.push(key);
    }
    return keys;
  }

  static getValues<T, K extends T[keyof T]>(type: T): K[] {
    const values: K[] = [];
    const filteredKeys = Object.keys(type)
      .filter((k) => {
        return !/\d+/.test(k);
      });
    for (let key of filteredKeys) {
      const converted = key as keyof T;
      const val = type[converted];
      if (typeof val === 'function') {
        continue;
      }
      values.push(<any>val);
    }
    return values;
  }
}
");

            var targetFile = GetTargetFile(targetPath);
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            File.WriteAllText(targetFile, sb.ToString(), CodeGenerator.Uft8WithoutBomEncoding);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        }

        internal string GetTargetFile(string targetPath)
        {
            return $"{targetPath}/{CodeGenerator.EnumsPath}/_enum-helper.ts";
        }
    }
}
