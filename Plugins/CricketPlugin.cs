using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using System.ComponentModel;


namespace ollamaDemo.Plugins
{
    using Microsoft.SemanticKernel;
    using System.ComponentModel;


    public class CricketPlugin
    {
        [KernelFunction("ExplainCricketTerm")]
        [Description(
            "Explains cricket terminology such as Yorker, LBW, Wide, " +
            "No Ball and Maiden Over. Use this for questions asking " +
            "what a cricket term means.")]
        public string ExplainCricketTerm(
            [Description("The cricket term to explain")]
        string term)
        {
            Console.WriteLine(
                $"[FUNCTION EXECUTED] ExplainCricketTerm | Term: {term}");

            return term.Trim().ToLowerInvariant() switch
            {
                "yorker" =>
                    "A yorker is a delivery that pitches close to the batter's feet.",

                "lbw" =>
                    "LBW means Leg Before Wicket. A batter can be dismissed " +
                    "when the ball hits their body instead of the bat and " +
                    "the conditions of the LBW law are satisfied.",

                "wide" =>
                    "A wide is an illegal delivery that is too far from " +
                    "the batter to play a normal shot.",

                "no ball" =>
                    "A no-ball is an illegal delivery. It usually gives " +
                    "the batting team one extra run.",

                "maiden over" =>
                    "A maiden over is an over in which no runs are " +
                    "charged to the bowler.",

                _ =>
                    $"No predefined explanation available for {term}."
            };
        }

        [KernelFunction("CalculateBattingStrikeRate")]
        [Description(
            "Calculates batting strike rate. Use when the user asks " +
            "for a batter's strike rate and provides runs scored and balls faced.")]
        public string CalculateBattingStrikeRate(
            [Description("Runs scored by the batter")]
        int runs,

            [Description("Balls faced by the batter")]
        int ballsFaced)
        {
            Console.WriteLine(
                $"[FUNCTION EXECUTED] CalculateBattingStrikeRate | " +
                $"Runs: {runs} | Balls: {ballsFaced}");

            if (runs < 0 || ballsFaced <= 0)
                return "Invalid input.";

            double strikeRate = (double)runs / ballsFaced * 100;

            return $"Batting strike rate: {strikeRate:F2}";
        }
    }

    //public class CricketPlugin
    //{
    //    [KernelFunction("ExplainCricketTerm")]
    //    [Description("Explains the meaning of a cricket term such as Yorker, LBW, Wide or No Ball.")]
    //    public string ExplainCricketTerm(
    //        [Description("The cricket term to explain")] string term)
    //    {
    //        Console.WriteLine(
    //            $"[FUNCTION EXECUTED] ExplainCricketTerm | Term: {term}");

    //        return term.Trim().ToLowerInvariant() switch
    //        {
    //            "yorker" =>
    //                "A yorker is a delivery that pitches close to the batter's feet or toes.",

    //            "lbw" =>
    //                "LBW means Leg Before Wicket. A batter can be dismissed if the ball hits their body instead of the bat and the conditions of the LBW law are satisfied.",

    //            "wide" =>
    //                "A wide is an illegal delivery that is too far from the batter to play a normal shot. It usually gives one extra run.",

    //            "no ball" =>
    //                "A no-ball is an illegal delivery. It usually gives one extra run and does not count as a legal delivery.",

    //            "maiden over" =>
    //                "A maiden over is an over in which the bowler concedes no runs from the batter.",

    //            _ =>
    //                $"I don't have a predefined explanation for {term}."
    //        };
    //    }

    //    [KernelFunction("CalculateBattingStrikeRate")]
    //    [Description("Calculates a batter's strike rate using runs scored and balls faced.")]
    //    public string CalculateBattingStrikeRate(
    //        [Description("Total runs scored by the batter")] int runs,
    //        [Description("Total legal deliveries faced by the batter")] int ballsFaced)
    //    {
    //        Console.WriteLine(
    //            $"[FUNCTION EXECUTED] CalculateBattingStrikeRate | Runs: {runs} | Balls: {ballsFaced}");

    //        if (runs < 0 || ballsFaced <= 0)
    //        {
    //            return "Invalid input. Runs cannot be negative and balls faced must be greater than zero.";
    //        }

    //        double strikeRate = (double)runs / ballsFaced * 100;

    //        return $"Strike rate is {strikeRate:F2}.";
    //    }
    //}
}
