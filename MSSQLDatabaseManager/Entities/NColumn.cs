using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;

namespace MSSQLDatabaseManager.Entities
{
    public class NColumn : NotificationObject
    {
        

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        private string _type;

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged(() => Type); 
                var at = _type.Replace("System.", "");
                switch (at)
                {
                    case "Boolean":
                        AType = $"bool{(Nullable ? "?" : "")}";
                        break;
                    case "Byte":
                        AType = $"byte{(Nullable ? "?" : "")}";
                        break;
                    case "Byte[]":
                        AType = $"data{(Nullable ? "?" : "")}";
                        break;
                    case "Char":
                        AType = $"char{(Nullable ? "?" : "")}";
                        break;
                    case "Decimal":
                        AType = $"decimal{(Nullable ? "?" : "")}";
                        break;
                    case "Double":
                        AType = $"double{(Nullable ? "?" : "")}";
                        break;
                    case "Single":
                        AType = $"float{(Nullable ? "?" : "")}";
                        break;
                    case "Int32":
                        AType = $"int{(Nullable ? "?" : "")}";
                        break;
                    case "Int64":
                        AType = $"long{(Nullable ? "?" : "")}";
                        break;
                    case "String":
                        AType = $"string{(Nullable ? "?" : "")}";
                        break;
                    case "DateTime":
                        AType = $"date{(Nullable ? "?" : "")}";
                        break;
                    default: AType = at;break;
                }
            }
        }

        private bool _nullable;

        public bool Nullable
        {
            get => _nullable;
            set
            {
                _nullable = value;
                RaisePropertyChanged(() => Nullable);
            }
        }

        private string _aType;
        [JsonIgnore]
        public string AType
        {
            get => _aType;
            set
            {
                _aType = value;
                RaisePropertyChanged(() => AType);
            }
        }

        public NColumn()
        {
            
        }

        public NColumn(string name, string type, bool nullable)
        {
            Name     = name;
            Nullable = nullable;
            Type     = type;
        }
        
    }
}
