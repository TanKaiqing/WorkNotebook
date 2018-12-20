using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SupplierWeb.Codes.EF
{
    public static class EfTools
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, Expression<Func<T, object>> keySelector, bool ascending)
        {
            var selectorBody = keySelector.Body;
            // Strip the Convert expression
            if (selectorBody.NodeType == ExpressionType.Convert)
                selectorBody = ((UnaryExpression)selectorBody).Operand;
            // Create dynamic lambda expression
            var selector = Expression.Lambda(selectorBody, keySelector.Parameters);
            // Generate the corresponding Queryable method call
            var queryBody = Expression.Call(typeof(Queryable),
                ascending ? "OrderBy" : "OrderByDescending",
                new Type[] { typeof(T), selectorBody.Type },
                source.Expression, Expression.Quote(selector));
            return source.Provider.CreateQuery<T>(queryBody);
        }

        public static IOrderedQueryable<T> OrderBy<T>(
            this IQueryable<T> query, string propertyName, bool ascending)
        {
            var entityType = typeof(T);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperty(propertyName);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                .Where(m => m.Name == (ascending ? "OrderBy" : "OrderByDescending") && m.IsGenericMethodDefinition)
                .Where(m =>
                {
                    var parameters = m.GetParameters().ToList();
                    //Put more restriction here to ensure selecting the right overload                
                    return parameters.Count == 2;//overload that has 2 parameters
                }).Single();

            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            MethodInfo genericMethod = method.MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<T>)genericMethod.Invoke(genericMethod, new object[] { query, selector });

            return newQuery;
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> query, string propertyName, dynamic value)
        {
            var entityType = typeof(T);
            var propertyInfo = entityType.GetProperty(propertyName);
            ParameterExpression parameter = Expression.Parameter(entityType, "x");
            ParameterExpression parameter2 = Expression.Parameter(entityType, "2");
            Expression.Equal(parameter, parameter2);
            MemberExpression property = Expression.Property(parameter, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { parameter });
        }
    }

}
