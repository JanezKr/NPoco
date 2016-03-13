using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NPoco
{
    public class PocoColumn
    {
        public PocoColumn()
        {
            ForceToUtc = true;
            MemberInfoChain = new List<MemberInfo>();
        }
        
        public static string GenerateKey(IEnumerable<MemberInfo> memberInfoChain)
        {
            return string.Join(PocoData.Separator, memberInfoChain.Select(x => x.Name).ToArray());
        }

        public TableInfo TableInfo;
        public string ColumnName;

        public List<MemberInfo> MemberInfoChain { get; set; }

        // JK
        // Add realation to MemberInfoChain if not exsist already
        public void AddRealation2MemberInfoChain(MemberInfo relation)
        {
            foreach (var member in MemberInfoChain)
            {
                if ((member.GetUnderlyingType() == relation) || (member == relation))
                {
                    return;
                }
            }

            MemberInfoChain.Insert(0, relation);
        }
        // JK

        private string _memberInfoKey;
        public string MemberInfoKey { get { return _memberInfoKey ?? (_memberInfoKey = GenerateKey(MemberInfoChain)); } }

        public MemberInfo MemberInfo { get; set; }

        public bool ResultColumn;
        public bool VersionColumn;
        public VersionColumnType VersionColumnType;
        public bool ComputedColumn;
        private Type _columnType;
        private MemberAccessor _memberAccessor;
        private List<MemberAccessor> _memberAccessorChain = new List<MemberAccessor>();

        public Type ColumnType
        {
            get { return _columnType ?? MemberInfo.GetMemberInfoType(); }
            set { _columnType = value; }
        }

        public bool ForceToUtc { get; set; }
        public string ColumnAlias { get; set; }

        public ReferenceType ReferenceType { get; set; }
        public bool SerializedColumn { get; set; }

        internal void SetMemberAccessors(List<MemberAccessor> memberAccessors)
        {
            _memberAccessor = memberAccessors.Last();
            _memberAccessorChain = memberAccessors;
        }

        public virtual void SetValue(object target, object val)
        {
            _memberAccessor.Set(target, val);
        }

        public virtual object GetValue(object target)
        {
            foreach (var memberAccessor in _memberAccessorChain)
            {
                target = target == null ? null : memberAccessor.Get(target);
            }
            //foreach (var memberInfo in MemberInfoChain)
            //{
            //    target = target == null ? null : memberInfo.GetMemberInfoValue(target);
            //}
            return target;
        }

        public virtual object ChangeType(object val) { return Convert.ChangeType(val, MemberInfo.GetMemberInfoType()); }

        public object GetColumnValue(PocoData pd, object target, Func<PocoColumn, object, object> callback = null)
        {
            callback = callback ?? ((_, o) => o);
            if (ReferenceType == ReferenceType.Foreign)
            {
                var member = pd.Members.Single(x => x.MemberInfo == MemberInfo);
                // JK
                var memberRferenceMemberNameFirst = member.ReferenceMemberName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).FirstOrDefault();
                // JK
                var column = member.PocoMemberChildren.SingleOrDefault(x => x.Name == memberRferenceMemberNameFirst);
                if (column == null)
                {
                    throw new Exception(string.Format("Could not find member on '{0}' with name '{1}'", member.MemberInfo.GetMemberInfoType(), member.ReferenceMemberName));
                }
                return callback(column.PocoColumn, column.PocoColumn.GetValue(target));
            }
            else
            {
                return callback(this, GetValue(target));
            }
        }
    }
}