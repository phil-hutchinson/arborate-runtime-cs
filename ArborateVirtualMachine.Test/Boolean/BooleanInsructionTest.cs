﻿using ArborateVirtualMachine.Entity;
using ArborateVirtualMachine.Exception;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static ArborateVirtualMachine.Entity.InstructionCode;

namespace ArborateVirtualMachine.Test.Boolean
{
    public class BooleanInsructionTest: BaseTest
    {
        #region InstructionCodes
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BooleanToStackExecutesCorrectly(bool value)
        {
            var inst = new List<Instruction>()
            {
                new Instruction(BooleanConstantToStack, value)
            };
            var actual = ExecuteBooleanFunction(inst);
            Assert.Equal(value, actual);
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void BooleanEqualExecutesCorrectly(bool val1, bool val2, bool expected)
        {
            var inst = new List<Instruction>()
            {
                new Instruction(BooleanConstantToStack, val1),
                new Instruction(BooleanConstantToStack, val2),
                new Instruction(BooleanEqual)
            };
            var actual = ExecuteBooleanFunction(inst);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public void BooleanNotEqualExecutesCorrectly(bool val1, bool val2, bool expected)
        {
            var inst = new List<Instruction>()
            {
                new Instruction(BooleanConstantToStack, val1),
                new Instruction(BooleanConstantToStack, val2),
                new Instruction(BooleanNotEqual)
            };
            var actual = ExecuteBooleanFunction(inst);
            Assert.Equal(expected, actual);
        }
        #endregion

        #region ThrownExceptions
        [Theory]
        [InlineData(VmType.Integer, VmType.Boolean, BooleanEqual)]
        [InlineData(VmType.Boolean, VmType.Integer, BooleanEqual)]
        [InlineData(VmType.Integer, VmType.Integer, BooleanEqual)]
        [InlineData(VmType.Integer, VmType.Boolean, BooleanNotEqual)]
        [InlineData(VmType.Boolean, VmType.Integer, BooleanNotEqual)]
        [InlineData(VmType.Integer, VmType.Integer, BooleanNotEqual)]
        public void BinaryInstructionWithIncorrectTypesOnStackThrows(VmType type1, VmType type2, InstructionCode instructionCode)
        {
            var instructions = new List<Instruction>()
            {
                BuildConstantToStackInstruction(type1),
                BuildConstantToStackInstruction(type2),
                new Instruction(instructionCode)
            };
            var exception = Assert.Throws<InvalidSourceException>(() => ExecuteBooleanFunction(instructions));

            Assert.Equal(InvalidSourceDetail.IncorrectElementTypeOnStack, exception.DetailCode);
        }

        [Theory]
        [InlineData(0, BooleanEqual)]
        [InlineData(1, BooleanEqual)]
        [InlineData(0, BooleanNotEqual)]
        [InlineData(1, BooleanNotEqual)]
        public void BooleanInstructionRequiringMoreElementsThanOnStackThrows(int numberOfValuesOnStack, InstructionCode instructionCode)
        {
            var instructions = new List<Instruction>();

            for (int i = 0; i < numberOfValuesOnStack; i++)
            {
                instructions.Add(new Instruction(BooleanConstantToStack, true));
            }

            instructions.Add(new Instruction(instructionCode));

            var exception = Assert.Throws<InvalidSourceException>(() => ExecuteBooleanFunction(instructions));

            Assert.Equal(InvalidSourceDetail.TooFewElementsOnStack, exception.DetailCode);
        }
        #endregion
    }
}