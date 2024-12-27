<script lang="ts">
  import * as Select from "$lib/components/ui/select/index.js";

  interface Props {
    initialValue: number;
    onPageSizeChange: (value: number) => void;
  }

  let { initialValue, onPageSizeChange }: Props = $props();

  const onValueChangeImpl = (newValue: string) => {
    onPageSizeChange(parseInt(newValue, 10));
  };
</script>

<Select.Root type="single" onValueChange={onValueChangeImpl}>
  <Select.Trigger class="w-24">{initialValue}</Select.Trigger>
  <Select.Content>
    <Select.Item value="10">10</Select.Item>
    <Select.Item value="20">20</Select.Item>
    <Select.Item value="50">50</Select.Item>
    <Select.Item value="100">100</Select.Item>
  </Select.Content>
</Select.Root>
