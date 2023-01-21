﻿using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.MainProgramParameterAggregation
{
    public interface IMainProgramPrameter
    {
        /// <summary>
        /// ツール径キー
        /// </summary>
        string DiameterKey { get; }

        /// <summary>
        /// ツール径
        /// </summary>
        decimal TargetToolDiameter { get; }

        /// <summary>
        /// C/D深さ
        /// </summary>
        decimal CenterDrillDepth { get; }

        /// <summary>
        /// 面取り深さ
        /// </summary>
        decimal? ChamferingDepth { get; }


        /// <summary>
        /// ドリル先端の長さ
        /// </summary>
        decimal DrillTipLength { get; }
    }

    /// <summary>
    /// リーマパラメータ
    /// </summary>
    /// <param name="DiameterKey">リーマ径</param>
    /// <param name="PreparedHoleDiameter">下穴1</param>
    /// <param name="SecondPreparedHoleDiameter">下穴2</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="ChamferingDepth">面取り深さ</param>
    public record class ReamingProgramPrameter(
        string DiameterKey,
        decimal PreparedHoleDiameter,
        decimal SecondPreparedHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth) : IMainProgramPrameter
    {
        [Logging]
        private static decimal Validate(string value) => decimal.Parse(value);

        public decimal TargetToolDiameter => Validate(DiameterKey);

        public decimal DrillTipLength => 5m;

        /// <summary>
        /// 下穴1のドリル先端の長さ
        /// </summary>
        public DrillTipLength FastPreparedHoleDrillTipLength => new(PreparedHoleDiameter);

        /// <summary>
        /// 下穴2のドリル先端の長さ
        /// </summary>
        public DrillTipLength SecondPreparedHoleDrillTipLength => new(SecondPreparedHoleDiameter);
    }

    public class TestReamingProgramPrameterFactory
    {
        // TODO: リストを見てデフォルト値を変更
        public static ReamingProgramPrameter Create(
            string DiameterKey = "12.3",
            decimal PreparedHoleDiameter = 9.1m,
            decimal SecondPreparedHoleDiameter = 11.1m,
            decimal CenterDrillDepth = 3.1m,
            decimal? ChamferingDepth = 1.7m) => new(DiameterKey, PreparedHoleDiameter, SecondPreparedHoleDiameter, CenterDrillDepth, ChamferingDepth);
    }

    /// <summary>
    /// タップパラメータ
    /// </summary>
    /// <param name="DiameterKey">タップ径</param>
    /// <param name="PreparedHoleDiameter">下穴</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="ChamferingDepth">面取り深さ</param>
    /// <param name="SpinForAluminum">回転(アルミ)</param>
    /// <param name="FeedForAluminum">送り(アルミ)</param>
    /// <param name="SpinForIron">回転(SS400)</param>
    /// <param name="FeedForIron">送り(SS400)</param>
    public record class TappingProgramPrameter(
        string DiameterKey,
        decimal PreparedHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron) : IMainProgramPrameter
    {
        [Logging]
        private static decimal Validate(string value)
        {
            var matchedDiameter = Regex.Match(value, @"(?<=M)\d+");
            if (!matchedDiameter.Success)
                throw new NCProgramConcatenationServiceException(
                    "タップ径の値が読み取れません\n" +
                    $"書式を確認してください タップ径: {value}");

            return decimal.Parse(matchedDiameter.Value);
        }

        public decimal TargetToolDiameter => Validate(DiameterKey);

        public decimal DrillTipLength => 5m;

        /// <summary>
        /// 下穴のドリル先端の長さ
        /// </summary>
        public DrillTipLength PreparedHoleDrillTipLength => new(PreparedHoleDiameter);
    }

    public class TestTappingProgramPrameterFactory
    {
        public static TappingProgramPrameter Create(
            string DiameterKey = "M12.3",
            decimal PreparedHoleDiameter = 11.1m,
            decimal CenterDrillDepth = 3.1m,
            decimal? ChamferingDepth = 1.7m,
            decimal SpinForAluminum = 700m,
            decimal FeedForAluminum = 300m,
            decimal SpinForIron = 700m,
            decimal FeedForIron = 300m) => new(DiameterKey, PreparedHoleDiameter, CenterDrillDepth, ChamferingDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
    }
    /// <summary>
    /// ドリルパラメータ
    /// </summary>
    /// <param name="DiameterKey">ドリル径</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="CutDepth">切込量</param>
    /// <param name="SpinForAluminum">回転(アルミ)</param>
    /// <param name="FeedForAluminum">送り(アルミ)</param>
    /// <param name="SpinForIron">回転(SS400)</param>
    /// <param name="FeedForIron">送り(SS400)</param>
    public record class DrillingProgramPrameter(
        string DiameterKey,
        decimal CenterDrillDepth,
        decimal CutDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron) : IMainProgramPrameter
    {
        [Logging]
        private static decimal Validate(string value) => decimal.Parse(value);

        [Logging]
        private static decimal CalcChamferingDepth(decimal diameter) => -(diameter / 2m + 0.2m);

        public decimal TargetToolDiameter => Validate(DiameterKey);

        public decimal? ChamferingDepth => CalcChamferingDepth(TargetToolDiameter);

        public decimal DrillTipLength => new DrillTipLength(TargetToolDiameter).Value;
    }

    public class TestDrillingProgramPrameterFactory
    {
        public static DrillingProgramPrameter Create(
        string DiameterKey="12.3",
        decimal CenterDrillDepth = 700m,
        decimal CutDepth = 700m,
        decimal SpinForAluminum = 700m,
        decimal FeedForAluminum = 700m,
        decimal SpinForIron = 700m,
        decimal FeedForIron = 700m) => new(DiameterKey, CenterDrillDepth, CutDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
    }
}
