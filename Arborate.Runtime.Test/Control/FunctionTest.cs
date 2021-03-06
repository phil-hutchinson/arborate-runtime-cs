﻿using Arborate.Runtime.Entity;
using Arborate.Runtime.Exception;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Arborate.Runtime.Entity.InstructionCode;

namespace Arborate.Runtime.Test.Control
{
    public class FunctionTest : BaseTest
    {
        #region InstructionCodes
        [Theory]
        [InlineData(5L, 25L)]
        [InlineData(10L, 100L)]
        public void VirtualMachineExecutesCallFunctionCorrectlyForOneParam(int paramValue, long expected)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, paramValue),
                    new Instruction(CallFunction, 1L),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(StackToVariable, 0L),
                    new Instruction(VariableToStack, 0L),
                    new Instruction(VariableToStack, 0L),
                    new Instruction(IntegerMultiply),
                },
                new List<VmType> { VmType.Integer },
                new List<VmType> { VmType.Integer },
                1
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            VmValue executionResult = vm.Execute()[0];

            long actual = ((VmInteger)executionResult).Val;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(5L, 3L, 2L)]
        [InlineData(10L, 18L, -8L)]
        public void VirtualMachineExecutesCallFunctionCorrectlyForMultiParam(int paramValue1, int paramValue2, long expected)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, paramValue1),
                    new Instruction(IntegerConstantToStack, paramValue2),
                    new Instruction(CallFunction, 1L),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerSubtract),
                },
                new List<VmType> { VmType.Integer, VmType.Integer },
                new List<VmType> { VmType.Integer },
                0
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            VmValue executionResult = vm.Execute()[0];

            long actual = ((VmInteger)executionResult).Val;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(5L, 0L, 1L)]
        [InlineData(5L, 3L, 125L)]
        [InlineData(2L, 10L, 1024L)]
        public void VirtualMachineExecutesCallFunctionCorrectlyForRecursive(int paramValue1, int paramValue2, long expected)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, paramValue1),
                    new Instruction(IntegerConstantToStack, paramValue2),
                    new Instruction(CallFunction, 1L),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(StackToVariable, 1L),
                    new Instruction(StackToVariable, 0L),
                    new Instruction(VariableToStack, 1L),
                    new Instruction(IntegerConstantToStack, 0L),
                    new Instruction(IntegerEqual),
                    new Instruction(BranchTrue, 14L),
                    new Instruction(VariableToStack, 0L),
                    new Instruction(VariableToStack, 1L),
                    new Instruction(IntegerConstantToStack, 1L),
                    new Instruction(IntegerSubtract),
                    new Instruction(CallFunction, 1L),
                    new Instruction(VariableToStack, 0L),
                    new Instruction(IntegerMultiply),
                    new Instruction(Branch, 15L),
                    new Instruction(IntegerConstantToStack, 1L),
                    new Instruction(IntegerConstantToStack, 1L), // this and next statement used as a NOP.
                    new Instruction(IntegerMultiply),
                },
                new List<VmType> { VmType.Integer, VmType.Integer },
                new List<VmType> { VmType.Integer },
                2
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            VmValue executionResult = vm.Execute()[0];

            long actual = ((VmInteger)executionResult).Val;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(20L, 5L, 1500L)]
        [InlineData(10L, 11L, -100L)]
        public void VirtualMachineExecutesCallFunctionCorrectlyWithItemsOnStack(int paramValue1, int paramValue2, long expected)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, 10),
                    new Instruction(IntegerConstantToStack, 10),
                    new Instruction(IntegerConstantToStack, paramValue1),
                    new Instruction(IntegerConstantToStack, paramValue2),
                    new Instruction(CallFunction, 1L),
                    new Instruction(IntegerMultiply),
                    new Instruction(IntegerMultiply),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerSubtract),
                },
                new List<VmType> { VmType.Integer, VmType.Integer },
                new List<VmType> { VmType.Integer },
                0
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            VmValue executionResult = vm.Execute()[0];

            long actual = ((VmInteger)executionResult).Val;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(100L, 5L, 48L, 12L, 16L)]
        [InlineData(33L, 3L, 56L, 2L, -17L)]
        public void CallFunctionBehavesCorrectlyWithMultipleReturnValues(int paramValue1, int paramValue2, int paramValue3, int paramValue4, long expected)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, paramValue1),
                    new Instruction(IntegerConstantToStack, paramValue2),
                    new Instruction(IntegerConstantToStack, paramValue3),
                    new Instruction(IntegerConstantToStack, paramValue4),
                    new Instruction(CallFunction, 1L),
                    new Instruction(IntegerSubtract),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerDivide),
                    new Instruction(StackToVariable, 0L),
                    new Instruction(IntegerDivide),
                    new Instruction(VariableToStack, 0L),
                },
                new List<VmType> { VmType.Integer, VmType.Integer, VmType.Integer, VmType.Integer },
                new List<VmType> { VmType.Integer, VmType.Integer },
                1
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            VmValue executionResult = vm.Execute()[0];

            long actual = ((VmInteger)executionResult).Val;

            Assert.Equal(expected, actual);
        }
        #endregion

        #region ThrownExceptions
        [Theory]
        [InlineData(-1L)]
        [InlineData(2L)]
        public void FunctionCallWithInvalidIndexThrows(long functionIndex)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, 1L),
                    new Instruction(IntegerConstantToStack, 2L),
                    new Instruction(CallFunction, functionIndex),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerMultiply),
                },
                new List<VmType> { VmType.Integer, VmType.Integer },
                new List<VmType> { VmType.Integer },
                0
            );

            var exception = Assert.Throws<InvalidSourceException>(() => new VirtualMachine(functionDefinition1, functionDefinition2));

            Assert.Equal(InvalidSourceDetail.InvalidFunctionIndex, exception.DetailCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void FunctionCallWithTooFewElementsOnStackThrows(int elementsOnStack)
        {
            var instructions1 = new List<Instruction>();
            for (int i = 0; i < elementsOnStack; i++)
            {
                instructions1.Add(new Instruction(IntegerConstantToStack, 1L));
            }
            instructions1.Add(new Instruction(CallFunction, 1L));
            var functionDefinition1 = new FunctionDefinition(
                instructions1,
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerAdd),
                    new Instruction(IntegerAdd),
                    new Instruction(IntegerAdd),
                },
                new List<VmType> { VmType.Integer, VmType.Integer, VmType.Integer, VmType.Integer },
                new List<VmType> { VmType.Integer },
                0
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            var exception = Assert.Throws<InvalidSourceException>(() => vm.Execute());

            Assert.Equal(InvalidSourceDetail.TooFewElementsOnStack, exception.DetailCode);
        }

        [Theory]
        [InlineData(VmType.Integer, VmType.Integer, VmType.Integer)]
        [InlineData(VmType.Boolean, VmType.Integer, VmType.Boolean)]
        [InlineData(VmType.Integer, VmType.Boolean, VmType.Boolean)]
        [InlineData(VmType.Boolean, VmType.Boolean, VmType.Boolean)]
        public void FunctionCallWithTooIncorrectElementTypeOnStackThrows(VmType type1, VmType type2, VmType type3)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    BuildConstantToStackInstruction(type1),
                    BuildConstantToStackInstruction(type2),
                    BuildConstantToStackInstruction(type3),
                    new Instruction(CallFunction, 1L)
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer, VmType.Integer, VmType.Boolean },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>(),
                new List<VmType> { VmType.Integer, VmType.Integer, VmType.Boolean },
                new List<VmType> { VmType.Integer },
                0
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            var exception = Assert.Throws<InvalidSourceException>(() => vm.Execute());

            Assert.Equal(InvalidSourceDetail.IncorrectCallArgumentType, exception.DetailCode);
        }

        [Theory]
        [InlineData(0, 0, 1, true)]
        [InlineData(0, 0, 2, true)]
        [InlineData(4, 0, 1, true)]
        [InlineData(4, 0, 2, true)]
        [InlineData(4, 2, 1, true)]
        [InlineData(4, 2, 2, true)]
        [InlineData(0, 0, 1, false)]
        [InlineData(0, 0, 2, false)]
        [InlineData(4, 0, 1, false)]
        [InlineData(4, 0, 2, false)]
        [InlineData(4, 2, 1, false)]
        [InlineData(4, 2, 2, false)]
        public void FunctionCallWithIncorrectArgumentCountOnExitThrows(int elementsOnStackPreCall, int inParamCount, int outParamCount, bool isTooFew)
        {
            // Note: if isTooFew is false, is too many.
            var instructions1 = new List<Instruction>();
            for (int i = 0; i < elementsOnStackPreCall; i++)
            {
                instructions1.Add(new Instruction(IntegerConstantToStack, 1L));
            }
            instructions1.Add(new Instruction(CallFunction, 1L));
            var functionDefinition1 = new FunctionDefinition(
                instructions1,
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var instructions2 = new List<Instruction>();
            var inParams2 = new List<VmType>();
            for (int i = 0; i < inParamCount; i++)
            {
                instructions2.Add(new Instruction(StackToVariable, 0L));
                inParams2.Add(VmType.Integer);
            }
            var outParams2 = new List<VmType>();
            for (int i = 0; i < outParamCount; i++)
            {
                outParams2.Add(VmType.Integer);
            }
            for (int i = 0; i < outParamCount + (isTooFew ? -1 : 1); i++)
            {
                instructions2.Add(new Instruction(IntegerConstantToStack, 0L));
            }
            var functionDefinition2 = new FunctionDefinition(
                instructions2,
                inParams2,
                outParams2,
                1
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            var exception = Assert.Throws<InvalidSourceException>(() => vm.Execute());

            Assert.Equal(InvalidSourceDetail.IncorrectReturnArgumentCount, exception.DetailCode);
        }

        [Theory]
        [InlineData(0, VmType.Integer, VmType.Integer, VmType.Integer)]
        [InlineData(0, VmType.Boolean, VmType.Integer, VmType.Boolean)]
        [InlineData(0, VmType.Integer, VmType.Boolean, VmType.Boolean)]
        [InlineData(0, VmType.Boolean, VmType.Boolean, VmType.Boolean)]
        [InlineData(0, VmType.Boolean, VmType.Integer, VmType.Integer)]
        [InlineData(2, VmType.Integer, VmType.Integer, VmType.Integer)]
        [InlineData(2, VmType.Boolean, VmType.Integer, VmType.Boolean)]
        [InlineData(2, VmType.Integer, VmType.Boolean, VmType.Boolean)]
        [InlineData(2, VmType.Boolean, VmType.Boolean, VmType.Boolean)]
        [InlineData(2, VmType.Boolean, VmType.Integer, VmType.Integer)]
        public void FunctionCallWithIncorrectArgumentTypesOnExitThrows(int elementsOnStackPreCall, VmType type1, VmType type2, VmType type3)
        {
            var instructions1 = new List<Instruction>();
            for (int i = 0; i < elementsOnStackPreCall; i++)
            {
                instructions1.Add(new Instruction(BooleanConstantToStack, true));
            }
            instructions1.Add(new Instruction(CallFunction, 1L));
            instructions1.Add(new Instruction(StackToVariable, 0L));
            instructions1.Add(new Instruction(StackToVariable, 0L));
            for (int i = 0; i < elementsOnStackPreCall; i++)
            {

            }
            var functionDefinition1 = new FunctionDefinition(
                instructions1,
                new List<VmType>(),
                new List<VmType> { VmType.Boolean },
                1
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>
                {
                    BuildConstantToStackInstruction(type1),
                    BuildConstantToStackInstruction(type2),
                    BuildConstantToStackInstruction(type3),
                },

                new List<VmType> { },
                new List<VmType> { VmType.Integer, VmType.Integer, VmType.Boolean },
                1
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            var exception = Assert.Throws<InvalidSourceException>(() => vm.Execute());

            Assert.Equal(InvalidSourceDetail.IncorrectReturnArgumentType, exception.DetailCode);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void FunctionCallDroppingBelowMinStackThrows(int stackDeficit)
        {
            // Note: if isTooFew is false, is too many.
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>
                {
                    new Instruction(IntegerConstantToStack, 5L),
                    new Instruction(IntegerConstantToStack, 6L),
                    new Instruction(CallFunction, 1L),
                },

                new List<VmType>(),
                new List<VmType> { VmType.Integer, VmType.Integer, VmType.Integer },
                0
            );

            var instructions2 = new List<Instruction>();
            for (int i = 0; i < stackDeficit; i++)
            {
                instructions2.Add(new Instruction(StackToVariable, 0L));
            }
            for (int i = 0; i < stackDeficit; i++)
            {
                instructions2.Add(new Instruction(IntegerConstantToStack, 10L));
            }
            instructions2.Add(new Instruction(IntegerConstantToStack, 0L));
            var functionDefinition2 = new FunctionDefinition(
                instructions2,
                new List<VmType> { },
                new List<VmType> { VmType.Integer },
                1
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            var exception = Assert.Throws<InvalidSourceException>(() => vm.Execute());

            Assert.Equal(InvalidSourceDetail.TooFewElementsOnStack, exception.DetailCode);
        }

        #endregion
    }
}
