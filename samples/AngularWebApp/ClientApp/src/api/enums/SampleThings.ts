// Generated by https://github.com/AdaskoTheBeAsT/TypeScriptGenerator

import { EnumHelper } from './_enum-helper';

export enum SampleThings {
  None = 0,
  FirstValue = 1,
  SecondValue = 2,
  ThirdValue = 4,
}

export namespace SampleThings {

  export function getLabel(value: SampleThings): string {
    switch(value) {
      case SampleThings.None: return 'Zero';
      case SampleThings.FirstValue: return 'FirstValue';
      case SampleThings.SecondValue: return 'SecondValue';
      case SampleThings.ThirdValue: return 'ThirdValue';
      default: throw new Error('Invalid value=`${value}`');
    }
  }

  export function getKeys(): string[] {
    return EnumHelper.getKeys(SampleThings);  
  }

  export function getValues(): SampleThings[] {
    return EnumHelper.getValues(SampleThings);
  }

  export function hasFlag(value: SampleThings, expected: SampleThings) {
    return (value && expected) === expected;
  }


}

