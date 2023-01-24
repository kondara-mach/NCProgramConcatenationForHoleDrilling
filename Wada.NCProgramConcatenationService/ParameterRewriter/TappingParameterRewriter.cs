﻿using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter.Process;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public class TappingParameterRewriter : IMainProgramParameterRewriter
    {
        [Logging]
        public IEnumerable<NCProgramCode> RewriteByTool(RewriteByToolRecord RewriteByToolRecord)
        {
            if (RewriteByToolRecord.Material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // タップのパラメータを受け取る
            var tappingParameters = RewriteByToolRecord.TapParameters;

            // ドリルのパラメータを受け取る
            var drillingParameters = RewriteByToolRecord.DrillingPrameters;

            // メインプログラムを工程ごとに取り出す
            List<NCProgramCode> ncPrograms = new();
            foreach (var rewritableCode in RewriteByToolRecord.RewritableCodes)
            {
                TappingProgramPrameter tappingParameter;
                try
                {
                    tappingParameter = tappingParameters
                        .First(x => x.TargetToolDiameter == RewriteByToolRecord.TargetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"タップ径 {RewriteByToolRecord.TargetToolDiameter}のリストがありません", ex);
                }

                switch (rewritableCode.MainProgramClassification)
                {
                    case NCProgramType.CenterDrilling:
                        ncPrograms.Add(CenterDrillingProgramRewriter.Rewrite(rewritableCode, RewriteByToolRecord.Material, tappingParameter));
                        break;
                    case NCProgramType.Drilling:
                        ncPrograms.Add(RewriteCNCProgramForDrilling(rewritableCode, RewriteByToolRecord.Material, RewriteByToolRecord.Thickness, drillingParameters, tappingParameter));
                        break;
                    case NCProgramType.Chamfering:
                        ncPrograms.Add(ChamferingProgramRewriter.Rewrite(rewritableCode, RewriteByToolRecord.Material, tappingParameter));
                        break;
                    case NCProgramType.Tapping:
                        ncPrograms.Add(TappingProgramRewriter.Rewrite(rewritableCode, RewriteByToolRecord.Material, RewriteByToolRecord.Thickness, tappingParameter));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return ncPrograms;
        }

        /// <summary>
        /// 下穴のパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="thickness"></param>
        /// <param name="drillingParameters"></param>
        /// <param name="tappingParameter"></param>
        /// <returns></returns>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        private static NCProgramCode RewriteCNCProgramForDrilling(NCProgramCode rewritableCode, MaterialType material, decimal thickness, IEnumerable<DrillingProgramPrameter> drillingParameters, TappingProgramPrameter tappingParameter)
        {
            var drillingParameter = drillingParameters
                .Where(x => x.TargetToolDiameter <= tappingParameter.PreparedHoleDiameter)
                .MaxBy(x => x.TargetToolDiameter);
            if (drillingParameter == null)
                throw new NCProgramConcatenationServiceException(
                    $"穴径に該当するリストがありません 穴径: {tappingParameter.PreparedHoleDiameter}");
            var hoge = DrillingProgramRewriter.Rewrite(rewritableCode, material, tappingParameter.PreparedHoleDiameter, thickness, drillingParameter);
            return hoge;
        }
    }
}
