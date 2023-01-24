﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.EditNCProgramApplication.Tests
{
    [TestClass()]
    public class EditNCProgramUseCaseTests
    {
        [DataTestMethod()]
        [DataRow(DirectedOperationTypeAttempt.Tapping, ReamerTypeAttempt.Undefined)]
        [DataRow(DirectedOperationTypeAttempt.Reaming, ReamerTypeAttempt.Crystal)]
        [DataRow(DirectedOperationTypeAttempt.Reaming, ReamerTypeAttempt.Skill)]
        [DataRow(DirectedOperationTypeAttempt.Drilling, ReamerTypeAttempt.Undefined)]
        public async Task 正常系_ユースケースを実行するとドメインサービスが実行されること(DirectedOperationTypeAttempt directedOperation, ReamerTypeAttempt reamer)
        {
            // given
            // when
            Mock<IMainProgramParameterRewriter> mock_crystal = new();
            Mock<IMainProgramParameterRewriter> mock_skill = new();
            Mock<IMainProgramParameterRewriter> mock_tap = new();
            Mock<IMainProgramParameterRewriter> mock_drill = new();

            var editNCProgramPram = TestEditNCProgramPramFactory.Create(
                directedOperation: directedOperation);

            IEditNCProgramUseCase editNCProgramUseCase =
                 new EditNCProgramUseCase(
                     (CrystalReamingParameterRewriter)mock_crystal.Object,
                     (SkillReamingParameterRewriter)mock_skill.Object,
                     (TappingParameterRewriter)mock_tap.Object,
                     (DrillingParameterRewriter)mock_drill.Object);
            _ = await editNCProgramUseCase.ExecuteAsync(editNCProgramPram);

            // then
            mock_crystal.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                reamer == ReamerTypeAttempt.Crystal && directedOperation == DirectedOperationTypeAttempt.Reaming ? Times.Once() : Times.Never());
            mock_skill.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                reamer == ReamerTypeAttempt.Skill && directedOperation == DirectedOperationTypeAttempt.Reaming ? Times.Once() : Times.Never());
            mock_tap.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                directedOperation == DirectedOperationTypeAttempt.Tapping ? Times.Once() : Times.Never());
            mock_drill.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                directedOperation == DirectedOperationTypeAttempt.Drilling ? Times.Once() : Times.Never());
        }
    }
}