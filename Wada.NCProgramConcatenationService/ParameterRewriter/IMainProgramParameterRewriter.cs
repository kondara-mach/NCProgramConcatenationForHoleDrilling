﻿using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter
{
    public interface IMainProgramParameterRewriter
    {
        /// <summary>
        /// メインプログラムのパラメータを書き換える
        /// </summary>
        /// <param name="rewriteByToolRecord"></param>
        /// <returns></returns>
        IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord);
    }

    /// <summary>
    /// RewriteByToolの引数用データクラス
    /// </summary>
    /// <param name="RewritableCodes">書き換え元NCプログラム</param>
    /// <param name="Material">素材</param>
    /// <param name="Thickness">板厚</param>
    /// <param name="SubProgramNumber">サブプログラム番号</param>
    /// <param name="DirectedOperationToolDiameter">目標工具径 :サブプログラムで指定した工具径</param>
    /// <param name="CrystalReamerParameters">クリスタルリーマパラメータ</param>
    /// <param name="SkillReamerParameters">スキルリーマパラメータ</param>
    /// <param name="TapParameters">タップパラメータ</param>
    /// <param name="DrillingPrameters">ドリルパラメータ</param>
    public record class RewriteByToolRecord(
        IEnumerable<NcProgramCode> RewritableCodes,
        MaterialType Material,
        decimal Thickness,
        string SubProgramNumber,
        decimal DirectedOperationToolDiameter,
        IEnumerable<ReamingProgramPrameter> CrystalReamerParameters,
        IEnumerable<ReamingProgramPrameter> SkillReamerParameters,
        IEnumerable<TappingProgramPrameter> TapParameters,
        IEnumerable<DrillingProgramPrameter> DrillingPrameters);

    public class TestRewriteByToolRecordFactory
    {
        public static RewriteByToolRecord Create(
            IEnumerable<NcProgramCode>? rewritableCodes = default,
            MaterialType material = MaterialType.Aluminum,
            decimal thickness = 12.3m,
            string subProgramNumber = "1000",
            decimal directedOperationToolDiameter = 13.3m,
            IEnumerable<ReamingProgramPrameter>? crystalReamerParameters = default,
            IEnumerable<ReamingProgramPrameter>? skillReamerParameters = default,
            IEnumerable<TappingProgramPrameter>? tapParameters = default,
            IEnumerable<DrillingProgramPrameter>? drillingPrameters = default)
        {
            rewritableCodes ??= new List<NcProgramCode>
            {
                TestNCProgramCodeFactory.Create(mainProgramType: NcProgramType.CenterDrilling),
                TestNCProgramCodeFactory.Create(mainProgramType: NcProgramType.Drilling),
                TestNCProgramCodeFactory.Create(mainProgramType: NcProgramType.Chamfering),
                TestNCProgramCodeFactory.Create(mainProgramType: NcProgramType.Reaming),
                TestNCProgramCodeFactory.Create(mainProgramType: NcProgramType.Tapping),
            };
            crystalReamerParameters ??= new List<ReamingProgramPrameter>
            {
                TestReamingProgramPrameterFactory.Create(),
            };
            skillReamerParameters ??= new List<ReamingProgramPrameter>
            {
                TestReamingProgramPrameterFactory.Create(),
            };
            tapParameters ??= new List<TappingProgramPrameter>
            {
                TestTappingProgramPrameterFactory.Create(),
            };
            drillingPrameters ??= new List<DrillingProgramPrameter>
            {
                TestDrillingProgramPrameterFactory.Create(
                    DiameterKey: "9.1",
                    CenterDrillDepth: -1.5m,
                    CutDepth: 2.5m,
                    SpinForAluminum: 1100,
                    FeedForAluminum: 130,
                    SpinForIron: 710,
                    FeedForIron: 100),
                TestDrillingProgramPrameterFactory.Create(
                    DiameterKey: "11.1",
                    CenterDrillDepth: -1.5m,
                    CutDepth: 3,
                    SpinForAluminum: 870,
                    FeedForAluminum: 110,
                    SpinForIron: 580,
                    FeedForIron: 80),
                TestDrillingProgramPrameterFactory.Create(),
                TestDrillingProgramPrameterFactory.Create(
                    DiameterKey: "15.3",
                    CenterDrillDepth: -1.5m,
                    CutDepth: 3.5m,
                    SpinForAluminum: 740,
                    FeedForAluminum: 100,
                    SpinForIron: 490,
                    FeedForIron: 70),
            };

            return new(rewritableCodes, material, thickness, subProgramNumber, directedOperationToolDiameter, crystalReamerParameters, skillReamerParameters, tapParameters, drillingPrameters);
        }
    }
    public enum MaterialType
    {
        Undefined,
        Aluminum,
        Iron,
    }

    public enum ParameterType
    {
        DrillParameter,
        CrystalReamerParameter,
        SkillReamerParameter,
        TapParameter,
    }
}