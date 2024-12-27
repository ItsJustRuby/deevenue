<script lang="ts">
  import * as Tooltip from "$lib/components/ui/tooltip/index.js";
  import { api } from "$lib/api/client";
  import Switch from "$lib/components/ui/switch/switch.svelte";

  import { session } from "$lib/store.svelte";

  let isSwitchVisible = $derived(session.isSfw !== null);
  let isSwitchChecked = $state<boolean | undefined>(session.isSfw ?? undefined);

  // Only runs once, when component is mounted.
  api.GET("/session").then((res) => {
    if (res.error) return;

    session.isSfw = res.data.isSfw;
    isSwitchChecked = res.data.isSfw;
  });

  $effect(() => {
    if (isSwitchChecked !== undefined && isSwitchChecked !== session.isSfw) {
      api
        .PATCH("/session", {
          body: {
            isSfw: isSwitchChecked,
          },
        })
        .then(() => {
          session.isSfw = isSwitchChecked!;
        });
    }
  });
</script>

<div>
  {#if isSwitchVisible}
    <Tooltip.Provider
      delayDuration={100}
      disableHoverableContent
      disableCloseOnTriggerClick
    >
      <Tooltip.Root>
        <Tooltip.Trigger>
          <Switch bind:checked={isSwitchChecked} />
        </Tooltip.Trigger>
        <Tooltip.Content interactOutsideBehavior="ignore"
          >SFW mode {isSwitchChecked ? "ON" : "OFF"}</Tooltip.Content
        >
      </Tooltip.Root>
    </Tooltip.Provider>
  {/if}
</div>
