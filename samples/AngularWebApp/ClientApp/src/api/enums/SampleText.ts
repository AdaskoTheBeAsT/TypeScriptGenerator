// Generated by https://github.com/AdaskoTheBeAsT/TypeScriptGenerator

import { EnumHelper } from './_enum-helper';

export enum SampleText {
  One = 'One',
  Two = 'Two',
}

export namespace SampleText {

  export function getLabel(value: SampleText): string {
    switch(value) {
      case SampleText.One: return 'Zero';
      case SampleText.Two: return 'Two';
      default: throw new Error('Invalid value=`${value}`');
    }
  }

  export function getKeys(): string[] {
    return EnumHelper.getKeys(SampleText);
  }

  export function getValues(): string[] {
    return EnumHelper.getValues(SampleText) as string[];
  }


}

