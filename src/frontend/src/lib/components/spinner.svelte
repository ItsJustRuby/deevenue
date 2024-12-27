<script lang="ts">
  import { cn } from "$lib/utils";
  import LoaderCircle, {
    type LoaderCircleProps,
  } from "lucide-svelte/icons/loader-circle";

  type SpinnerProps = LoaderCircleProps;

  const { class: className, ...restProps }: SpinnerProps = $props();
</script>

<LoaderCircle
  class={cn(
    "col-start-1 col-end-1 row-start-1 row-end-1 animate-spin",
    className,
  )}
  {...restProps}
/>
