using Arborate.Runtime.Entity;
using static Arborate.Runtime.Entity.InstructionCode;
using System;
using System.Collections.Generic;
using Xunit;
using Arborate.Runtime.Exception;
using System.Linq;

namespace Arborate.Runtime.Test
{
    public class VirtualMachineTest: BaseTest
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        public void FunctionWithIncorrectReturnArgumentCountThrows(int stackCount, int outParamCount)
        {
            var inst = Enumerable.Repeat(0, stackCount).Select(dummy => new Instruction(BooleanConstantToStack, true)).ToList();
            var outParams = Enumerable.Repeat(VmType.Boolean, outParamCount).ToList();

            var exception = Assert.Throws<InvalidSourceException>(() => ExecuteFunction(inst, outParams: outParams));

            Assert.Equal(InvalidSourceDetail.IncorrectReturnArgumentCount, exception.DetailCode);
        }

        public static IEnumerable<object[]> MemberData_FunctionWithIncorrectReturnArgumentTypeThrows =>
            new List<object[]>
            {
                new object[] {new VmType[] { VmType.Boolean }, new VmType[] { VmType.Integer }},
                new object[] {new VmType[] { VmType.Integer }, new VmType[] { VmType.Boolean }},
                new object[] {new VmType[] { VmType.Boolean, VmType.Boolean }, new VmType[] { VmType.Boolean, VmType.Integer }},
                new object[] {new VmType[] { VmType.Boolean, VmType.Boolean }, new VmType[] { VmType.Integer, VmType.Integer }},
                new object[] {new VmType[] { VmType.Integer, VmType.Boolean }, new VmType[] { VmType.Boolean, VmType.Integer }},
            };

        [Theory]
        [MemberData(nameof(MemberData_FunctionWithIncorrectReturnArgumentTypeThrows))]
        public void FunctionWithIncorrectReturnArgumentTypeThrows(VmType[] outParams, VmType[] stackParams)
        {
            var instructions = new List<Instruction>();
            foreach(var vmType in Enumerable.Reverse(stackParams))
            {
                switch (vmType)
                {
                    case VmType.Boolean:
                        instructions.Add(new Instruction(BooleanConstantToStack, true));
                        break;

                    case VmType.Integer:
                        instructions.Add(new Instruction(IntegerConstantToStack, 100L));
                        break;

                    default:
                        Assert.True(false); // invalid data for test.
                        break;
                }
            }

            var exception = Assert.Throws<InvalidSourceException>(() => ExecuteFunction(instructions, outParams: outParams));

            Assert.Equal(InvalidSourceDetail.IncorrectReturnArgumentType, exception.DetailCode);
        }

        [Fact]
        public void FunctionWithoutReturnTypeThrowsOnVirtualMachineCreation()
        {
            var instructions = new List<Instruction>()
            {
                new Instruction(BooleanConstantToStack, true),
            };

            var functionDefinition = new FunctionDefinition(instructions, new List<VmType>(), new List<VmType>(), 0);
            var exception = Assert.Throws<InvalidSourceException>(() => new VirtualMachine(functionDefinition));

            Assert.Equal(InvalidSourceDetail.FunctionDefinitionMissingReturnValue, exception.DetailCode);
        }

        [Theory]
        [InlineData(null, 9L)]
        [InlineData(0, 9L)]
        [InlineData(1, 27L)]
        public void VirtualMachineExecutesCorrectFunction(int? functionToExecute, long expected)
        {
            var functionDefinition1 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, 3L),
                    new Instruction(IntegerConstantToStack, 3L),
                    new Instruction(IntegerMultiply),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var functionDefinition2 = new FunctionDefinition(
                new List<Instruction>()
                {
                    new Instruction(IntegerConstantToStack, 3L),
                    new Instruction(IntegerConstantToStack, 3L),
                    new Instruction(IntegerMultiply),
                    new Instruction(IntegerConstantToStack, 3L),
                    new Instruction(IntegerMultiply),
                },
                new List<VmType>(),
                new List<VmType> { VmType.Integer },
                0
            );

            var vm = new VirtualMachine(functionDefinition1, functionDefinition2);

            VmValue executionResult = vm.Execute(functionToExecute ?? 0);

            long actual = ((VmInteger)executionResult).Val;

            Assert.Equal(expected, actual);
        }


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

            VmValue executionResult = vm.Execute();

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

            VmValue executionResult = vm.Execute();

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

            VmValue executionResult = vm.Execute();

            long actual = ((VmInteger)executionResult).Val;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-3)]
        public void FunctionWithInvalidInstructionCodesThrows(int instructionCode)
        {
            var instructions = new List<Instruction>()
            {
                new Instruction((InstructionCode)instructionCode),
            };
            var functionDefinition = new FunctionDefinition(instructions, new List<VmType>(), new List<VmType>() { VmType.Boolean }, 0);
            var exception = Assert.Throws<InvalidSourceException>(() => new VirtualMachine(functionDefinition));

            Assert.Equal(InvalidSourceDetail.InvalidInstruction, exception.DetailCode);
        }

        [Theory]
        [InlineData(BooleanConstantToStack)]
        [InlineData(IntegerConstantToStack)]
        [InlineData(Branch)]
        [InlineData(BranchTrue)]
        [InlineData(BranchFalse)]
        [InlineData(StackToVariable)]
        [InlineData(VariableToStack)]
        public void InstructionMissingRequiredDataThrows(InstructionCode instructionCode)
        {
            var instructions = new List<Instruction>()
            {
                new Instruction(instructionCode)
            };

            var functionDefinition = new FunctionDefinition(instructions, new List<VmType>(), new List<VmType>() { VmType.Boolean }, 0);
            var exception = Assert.Throws<InvalidSourceException>(() => new VirtualMachine(functionDefinition));

            Assert.Equal(InvalidSourceDetail.MissingInstructionData, exception.DetailCode);
        }

        [Theory]
        [InlineData(BooleanConstantToStack, VmType.Integer)]
        [InlineData(IntegerConstantToStack, VmType.Boolean)]
        [InlineData(Branch, VmType.Boolean)]
        [InlineData(BranchTrue, VmType.Boolean)]
        [InlineData(BranchFalse, VmType.Boolean)]
        [InlineData(StackToVariable, VmType.Boolean)]
        [InlineData(VariableToStack, VmType.Boolean)]
        public void InstructionRequiringDataWithInvalidTypeThrows(InstructionCode instructionCode, VmType vmType)
        {

            var instructions = new List<Instruction>();

            switch(vmType)
            {
                case VmType.Integer:
                    instructions.Add(new Instruction(instructionCode, 0L));
                    break;

                case VmType.Boolean:
                    instructions.Add(new Instruction(instructionCode, true));
                    break;

                default:
                    Assert.True(false); // error in test;
                    break;
            }

            var functionDefinition = new FunctionDefinition(instructions, new List<VmType>(), new List<VmType>() { VmType.Boolean }, 0);
            var exception = Assert.Throws<InvalidSourceException>(() => new VirtualMachine(functionDefinition));

            Assert.Equal(InvalidSourceDetail.InvalidInstructionData, exception.DetailCode);
        }

        [Theory]
        [InlineData(BooleanEqual)]
        [InlineData(BooleanNotEqual)]
        [InlineData(BooleanAnd)]    
        [InlineData(BooleanOr)]
        [InlineData(BooleanNot)]
        [InlineData(IntegerEqual)]
        [InlineData(IntegerNotEqual)]
        [InlineData(IntegerAdd)]
        [InlineData(IntegerSubtract)]
        [InlineData(IntegerMultiply)]
        [InlineData(IntegerDivide)]
        [InlineData(IntegerModulus)]
        public void InstructionWithUnnecessaryDataThrows(InstructionCode instructionCode)
        {
            var instructions = new List<Instruction>()
            {
                new Instruction(instructionCode, 0L)
            };

            var functionDefinition = new FunctionDefinition(instructions, new List<VmType>(), new List<VmType>() { VmType.Boolean }, 0);
            var exception = Assert.Throws<InvalidSourceException>(() => new VirtualMachine(functionDefinition));

            Assert.Equal(InvalidSourceDetail.InstructionCodeDoesNotUseData, exception.DetailCode);
        }

    }
}
