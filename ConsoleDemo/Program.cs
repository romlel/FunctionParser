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
            ConnectorsClass Connectors = new ConnectorsClass();
            Connectors.IsProperId = (a) => a.StartsWith("rlel");
            Connectors.ExecuteFunction = (a, b) => "42";
            Connectors.GetIdValue = a => "Value of " + a;


            while (true)
            {
                string[] idsNames = new string[] { };
                string[] idsValues = new string[] { };

                var txt_expr = Console.ReadLine();

                Console.WriteLine(Expression.Compute(txt_expr, Connectors));

/*
                if (Statics.IsExpression(Connectors, txt_exp))
                {

                    Expression expression = new Expression(Connectors, txt_expr, idsNames, null);
                    //TreeNode expr = new TreeNode(txt_expr.Text);
                    //CreateTreeNodes(expression, expr);
                    //trvw_ParseTree.Nodes.Clear();
                    //trvw_ParseTree.Nodes.Add(expr);
                    //trvw_ParseTree.ExpandAll();
                    //expression.ad
                    object result = expression.CalculateValue(Connectors);

                    Console.WriteLine(result.ToString());
                    //lbl_result.Text = result.ToString();// string.Format("= {0:f3}", result);
                }
                else
                {
                    Console.WriteLine("ERROR");
                }*/

            }
        }
    }
}
