﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using DMX.Gatekeeper.Api.Models.Labs;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using Xunit;

namespace DMX.Gatekeeper.Api.Tests.Unit.Services.Foundations.Labs
{
    public partial class LabServiceTests
    {
        [Fact]
        public async Task ShouldAddLabsAsync()
        {
            // given
            Lab randomLab = CreateRandomLab();
            Lab inputLab = randomLab;
            Lab postedLab = inputLab;
            Lab expectedLab = postedLab.DeepClone();

            this.dmxApiBrokerMock.Setup(broker =>
                broker.PostLabAsync(inputLab))
                    .ReturnsAsync(postedLab);

            // when
            Lab actualLab = 
                await this.labService.AddLabAsync(inputLab);

            // then
            actualLab.Should().BeEquivalentTo(expectedLab);

            this.dmxApiBrokerMock.Verify(broker =>
                broker.PostLabAsync(inputLab),
                    Times.Once);

            this.dmxApiBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
