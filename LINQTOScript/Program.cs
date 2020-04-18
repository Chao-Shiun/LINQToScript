using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Linq.Expressions;
using System.Text;
using static LINQTOScript.Customers;

namespace LINQTOScript
{
    class Program
    {
        static void Main(string[] args)
        {
            static void Main(string[] args)
            {
                var test = BenchmarkRunner.Run<TestClass>();
                //var test=new SQLContext<Customers>().
                //    Where(x => x.NAME == "QXA" && x.AGE == 52 && x.ID == 12 || x.Address == "Taipei City" && x.Gender == Sex.Male);

                Console.ReadKey();
            }
        }
    }
    [MemoryDiagnoser]
    public class TestClass
    {
        SQLContext<Customers> context;
        public TestClass()
        {
            context = new SQLContext<Customers>();
        }
        [Benchmark]
        public string Test1() => context.Where(x => x.AGE == 10);
        [Benchmark]
        public string Test2() => context.Where(x => x.NAME == "QXA");
        [Benchmark]
        public string Test3() => context.Where(x => x.NAME == "QXA" && x.AGE == 52);
        [Benchmark]
        public string Test4() => context.Where(x => x.NAME == "QXA" && x.AGE == 52 && x.ID == 12);
        [Benchmark]
        public string Test5() => context.Where(x => x.NAME == "QXA" && x.AGE == 52 && x.ID == 12 || x.Address == "Taipei City");
        [Benchmark]
        public string Test6() => context.Where(x => x.NAME == "QXA" && x.AGE == 52 && x.ID == 12 || x.Address == "Taipei City" && x.Gender == Sex.Male);
    }

    [MemoryDiagnoser]
    public class SQLContext<T>
    {
        private string _tableName;

        public SQLContext()
        {
            _tableName = typeof(T).Name;
        }

        public string Where(Expression<Func<T, bool>> expression)
        {
            StringBuilder sb = new StringBuilder($"Select * From {_tableName} WHERE ");
            ProcessExpression(expression, sb);
            return sb.ToString();
        }

        static void ProcessExpression(Expression expression, StringBuilder sb)
        {
            if (expression.NodeType == ExpressionType.Equal)
            {
                ProcessExpression(((BinaryExpression)expression).Left, sb);
                sb.Append(" = ");
                ProcessExpression(((BinaryExpression)expression).Right, sb);
            }

            if (expression.NodeType == ExpressionType.AndAlso)
            {
                ProcessExpression(((BinaryExpression)expression).Left, sb);
                sb.Append(" AND ");
                ProcessExpression(((BinaryExpression)expression).Right, sb);
            }

            if (expression.NodeType == ExpressionType.LessThan)
            {
                BinaryExpression expr = (BinaryExpression)expression;
                ProcessExpression(expr.Left, sb);
                sb.Append(" < ");
                ProcessExpression(expr.Right, sb);
            }

            if (expression.NodeType == ExpressionType.GreaterThan)
            {
                BinaryExpression expr = (BinaryExpression)expression;
                ProcessExpression(expr.Left, sb);
                sb.Append(" > ");
                ProcessExpression(expr.Right, sb);
            }

            if (expression.NodeType == ExpressionType.Or)
            {
                BinaryExpression expr = (BinaryExpression)expression;
                ProcessExpression(expr.Left, sb);
                sb.Append(" Or ");
                ProcessExpression(expr.Right, sb);
            }

            if (expression.NodeType == ExpressionType.OrElse)
            {
                BinaryExpression expr = (BinaryExpression)expression;
                ProcessExpression(expr.Left, sb);
                sb.Append(" Or ");
                ProcessExpression(expr.Right, sb);
            }

            if (expression is UnaryExpression)
            {
                UnaryExpression uExp = expression as UnaryExpression;
                ProcessExpression(uExp.Operand, sb);
            }
            else if (expression is LambdaExpression)
            {
                ProcessExpression(((LambdaExpression)expression).Body, sb);
            }
            else if (expression is MethodCallExpression)
            {
                sb.Append(" ");
                sb.Append(((MethodCallExpression)expression).Method.DeclaringType.ToString() + ".");
                sb.Append(((MethodCallExpression)expression).Method.Name);
                sb.Append("(");
                foreach (object param in ((MethodCallExpression)expression).Arguments)
                {
                    if (param is Expression)
                        ProcessExpression((Expression)param, sb);
                    else
                        sb.Append(param.ToString());
                    sb.Append(",");
                }
                sb = sb.Remove(sb.Length - 1, 1);
                sb.Append(") ");
            }
            else if (expression is MemberExpression)
            {
                sb.Append(((MemberExpression)expression).Member.Name);
            }
            else if (expression is ConstantExpression)
            {
                if (((ConstantExpression)expression).Value is Expression)
                    ProcessExpression((Expression)((ConstantExpression)expression).Value, sb);
                object value = ((ConstantExpression)expression).Value;
                if (value is string)
                    sb.Append("'");
                sb.Append(((ConstantExpression)expression).Value.ToString());
                if (value is string)
                    sb.Append("'");
            }
        }
    }

    public class Customers
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public int AGE { get; set; }
        public string Address { get; set; }
        public Sex Gender { get; set; }

        public enum Sex
        {
            Male,
            Female
        }
    }
}
