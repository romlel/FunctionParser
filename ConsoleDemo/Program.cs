using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionParser;

namespace ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Connectors.IsProperId = (a) => a.StartsWith("rlel");
            Connectors.ExecuteFunction = (a, b) => "42";
            Connectors.GetIdValue = a => "Value of " + a;


            while (true)
            {
                string[] idsNames = new string[] { };
                string[] idsValues = new string[] { };

                var txt_expr = Console.ReadLine();

                if (Expression.IsExpression(txt_expr/*, idsNames*/))
                {

                    Expression expression = new Expression(txt_expr, idsNames, null);
                    //TreeNode expr = new TreeNode(txt_expr.Text);
                    //CreateTreeNodes(expression, expr);
                    //trvw_ParseTree.Nodes.Clear();
                    //trvw_ParseTree.Nodes.Add(expr);
                    //trvw_ParseTree.ExpandAll();
                    //expression.ad
                    object result = expression.CalculateValue();

                    Console.WriteLine(result.ToString());
                    //lbl_result.Text = result.ToString();// string.Format("= {0:f3}", result);
                }
                else
                {
                    Console.WriteLine("ERROR");
                }

            }
        }
    }
}
