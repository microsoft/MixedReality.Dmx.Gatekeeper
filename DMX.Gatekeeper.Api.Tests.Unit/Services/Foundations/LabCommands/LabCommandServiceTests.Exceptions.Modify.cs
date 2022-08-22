﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DMX.Gatekeeper.Api.Models.LabCommands;
using DMX.Gatekeeper.Api.Models.LabCommands.Exceptions;
using FluentAssertions;
using Moq;
using RESTFulSense.Exceptions;
using Xeptions;
using Xunit;

namespace DMX.Gatekeeper.Api.Tests.Unit.Services.Foundations.LabCommands
{
    public partial class LabCommandServiceTests
    {
        [Theory]
        [MemberData(nameof(CriticalDependencyException))]
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfCriticalErrorOccursAndLogItAsync(
            Xeption criticalDependencyException)
        {
            // given
            LabCommand someLabCommand = CreateRandomLabCommand();

            var failedLabCommandDependencyException =
                new FailedLabCommandDependencyException(criticalDependencyException);

            var expectedLabCommandDependencyException =
                new LabCommandDependencyException(failedLabCommandDependencyException);

            this.dmxApiBrokerMock.Setup(broker =>
                broker.UpdateLabCommandAsync(It.IsAny<LabCommand>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            ValueTask<LabCommand> modifyLabCommandTask =
                this.labCommandService.ModifyLabCommandAsync(someLabCommand);

            LabCommandDependencyException actualLabCommandDependencyException =
                await Assert.ThrowsAsync<LabCommandDependencyException>(
                    modifyLabCommandTask.AsTask);

            // then
            actualLabCommandDependencyException.Should().BeEquivalentTo(
                expectedLabCommandDependencyException);

            this.dmxApiBrokerMock.Verify(broker =>
                broker.UpdateLabCommandAsync(It.IsAny<LabCommand>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedLabCommandDependencyException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dmxApiBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfErrorsOccurAndLogItAsync()
        {
            // given
            LabCommand someLabCommand = CreateRandomLabCommand();
            string randomMessage = GetRandomString();
            var httpResponseMessage = new HttpResponseMessage();

            var httpResponseException =
                new HttpResponseException(httpResponseMessage, randomMessage);

            var failedLabCommandDependencyException =
                new FailedLabCommandDependencyException(httpResponseException);

            var expectedLabCommandDependencyException =
                new LabCommandDependencyException(failedLabCommandDependencyException);

            this.dmxApiBrokerMock.Setup(broker =>
                broker.UpdateLabCommandAsync(It.IsAny<LabCommand>()))
                    .ThrowsAsync(httpResponseException);

            // when
            ValueTask<LabCommand> modifyLabCommandTask =
                this.labCommandService.ModifyLabCommandAsync(someLabCommand);

            LabCommandDependencyException actualLabCommandDependencyException =
                await Assert.ThrowsAsync<LabCommandDependencyException>(
                    modifyLabCommandTask.AsTask);

            // then
            actualLabCommandDependencyException.Should().BeEquivalentTo(
                expectedLabCommandDependencyException);

            this.dmxApiBrokerMock.Verify(broker =>
                broker.UpdateLabCommandAsync(It.IsAny<LabCommand>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(
                    SameExceptionAs(expectedLabCommandDependencyException))),
                        Times.Once);

            this.dmxApiBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            LabCommand somelabCommand = CreateRandomLabCommand();
            string randomMessage = GetRandomString();
            var httpMessage = new HttpResponseMessage();

            Dictionary<string, List<string>> randomDictionary =
                CreateRandomDictionary();

            var httpResponseBadRequestException =
                new HttpResponseBadRequestException(
                    httpMessage,
                    randomMessage);

            httpResponseBadRequestException.AddData(randomDictionary);

            var invalidPutException =
                new InvalidLabCommandException(
                    httpResponseBadRequestException,
                    randomDictionary);

            var expectedLabCommandDependencyValidationException =
                new LabCommandDependencyValidationException(invalidPutException);

            this.dmxApiBrokerMock.Setup(broker =>
                broker.UpdateLabCommandAsync(somelabCommand))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            ValueTask<LabCommand> modifyLabCommandTask =
                this.labCommandService.ModifyLabCommandAsync(somelabCommand);

            LabCommandDependencyValidationException actualLabCommandDependencyValidationException =
                await Assert.ThrowsAsync<LabCommandDependencyValidationException>(
                    modifyLabCommandTask.AsTask);

            // then
            actualLabCommandDependencyValidationException.Should().BeEquivalentTo(
                expectedLabCommandDependencyValidationException);

            this.dmxApiBrokerMock.Verify(broker =>
                broker.UpdateLabCommandAsync(It.IsAny<LabCommand>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedLabCommandDependencyValidationException))),
                        Times.Once);

            this.dmxApiBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfLabCommandNotFoundOccursAndLogItAsync()
        {
            // given
            LabCommand someLabCommand = CreateRandomLabCommand();
            string randomMessage = GetRandomString();

            var httpResponseMessage = new HttpResponseMessage();

            var httpResponseNotFoundException =
                new HttpResponseNotFoundException(
                    httpResponseMessage,
                    randomMessage);

            var notFoundLabCommandException =
                new NotFoundLabCommandException(httpResponseNotFoundException);

            var expectedLabCommandDependencyValidationException =
                new LabCommandDependencyValidationException(notFoundLabCommandException);

            this.dmxApiBrokerMock.Setup(broker =>
                broker.UpdateLabCommandAsync(It.IsAny<LabCommand>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            ValueTask<LabCommand> modifyLabCommandTask =
                this.labCommandService.ModifyLabCommandAsync(someLabCommand);

            LabCommandDependencyValidationException actualLabCommandDependencyValidationException =
                await Assert.ThrowsAsync<LabCommandDependencyValidationException>(
                    modifyLabCommandTask.AsTask);

            // then
            actualLabCommandDependencyValidationException.Should().BeEquivalentTo(
                expectedLabCommandDependencyValidationException);

            this.dmxApiBrokerMock.Verify(broker =>
                broker.UpdateLabCommandAsync(It.IsAny<LabCommand>()),
                   Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedLabCommandDependencyValidationException))),
                        Times.Once);

            this.dmxApiBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
