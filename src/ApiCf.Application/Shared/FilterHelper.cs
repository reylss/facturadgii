using Abp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ApiCf.SharedNs
{
    public static class FilterHelper<T>
    {
        public static IQueryable<T> FilerByProperties(string properiesFilter, IQueryable<T> query)
        {
            if (properiesFilter == null) return query;
            string[] properties = properiesFilter.Split(";");

            var filters = new List<DynamicFilter>();
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properiesFilter.IsNullOrEmpty())
                {
                    int pocisionProp = properties[i].ToString().IndexOf('=');
                    string prop = properties[i].Substring(0, pocisionProp);
                    object value = properties[i].Substring(pocisionProp + 1);

                    if (typeof(T).GetProperty(prop) != null)
                    {
                        if (Int32.TryParse(value.ToString(), out Int32 badWayBuySomeWay))
                        {
                            value = badWayBuySomeWay;
                        }
                        if (typeof(T).GetProperty(prop).PropertyType.Name == "Boolean")
                        {
                            value = Convert.ToBoolean(value);
                        }
                        var filter = new DynamicFilter { PropertyName = prop, Value = value };
                        filters.Add(filter);
                    }
                }
            }

            if (filters.Count > 0)
            {
                var deleg = ExpressionBuilder.GetExpression<T>(filters).Compile();
                return query.Where(deleg).AsQueryable();
            }

            return query;
        }
    }


    public static class ExpressionBuilder
    {
        public static Expression<Func<T, bool>> GetExpression<T>(IList<DynamicFilter> filters)
        {
            if (filters.Count == 0)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(T), "t");
            Expression exp = null;

            if (filters.Count == 1)
            {
                exp = GetExpression<T>(param, filters[0]);
            }
            else if (filters.Count == 2)
            {
                exp = GetExpression<T>(param, filters[0], filters[1]);
            }
            else
            {
                while (filters.Count > 0)
                {
                    var f1 = filters[0];
                    var f2 = filters[1];

                    exp = exp == null
                        ? GetExpression<T>(param, filters[0], filters[1])
                        : Expression.AndAlso(exp, GetExpression<T>(param, filters[0], filters[1]));

                    filters.Remove(f1);
                    filters.Remove(f2);

                    if (filters.Count == 1)
                    {
                        exp = Expression.AndAlso(exp, GetExpression<T>(param, filters[0]));
                        filters.RemoveAt(0);
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(exp, param);
        }

        private static Expression GetExpression<T>(ParameterExpression param, DynamicFilter filter)
        {
            MemberExpression member = Expression.Property(param, filter.PropertyName);
            ConstantExpression constant = Expression.Constant(filter.Value);

            return Expression.Equal(member, constant);
        }

        private static BinaryExpression GetExpression<T>(ParameterExpression param, DynamicFilter filter1, DynamicFilter filter2)
        {
            Expression bin1 = GetExpression<T>(param, filter1);
            Expression bin2 = GetExpression<T>(param, filter2);

            return Expression.AndAlso(bin1, bin2);
        }
    }

    public class DynamicFilter
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }
}




