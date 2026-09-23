import { BaseCombobox, type OptionItem, type BaseComboboxProps } from './BaseCombobox'
import { useState } from 'react'

type DistributiveOmit<T, K extends PropertyKey> = T extends unknown ? Omit<T, K> : never;

export type StaticComboboxProps<TValue = unknown> = DistributiveOmit<
    BaseComboboxProps<TValue>,
    'items' | 'onSearch'
> & {
    options: OptionItem<TValue>[]
    isLoading?: boolean
    loadingText?: string
}

/**
 * StaticCombobox
 * Component xử lý filter phía client cho dữ liệu tĩnh.
 * Độc lập hoàn toàn với Form.
 */
export function StaticCombobox<TValue = unknown>({
    options,
    isLoading = false,
    loadingText = 'Loading...',
    emptyText = 'No results found.',
    onValueChange,
    ...rest
}: StaticComboboxProps<TValue>) {
    const [search, setSearch] = useState('');

    const filteredOptions = (options || []).filter(item => 
        item.label.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <BaseCombobox
            {...rest as any}
            items={filteredOptions}
            onValueChange={onValueChange as any}
            onSearch={setSearch}
            emptyText={isLoading ? loadingText : emptyText}
        />
    )
}
