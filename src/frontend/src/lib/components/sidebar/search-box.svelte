<script lang="ts">
  import * as Sidebar from "$lib/components/ui/sidebar/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import { goto } from "$app/navigation";
  import { searchState } from "$lib/store.svelte";

  // Locally editable, but overriden by any changes
  // incoming from `searchState`.
  let localSearchTerm = $state(searchState.searchTerm);

  $effect(() => {
    localSearchTerm = searchState.searchTerm;
  });

  const sidebar = Sidebar.useSidebar();

  const submit = async (e: SubmitEvent) => {
    e.preventDefault();
    sidebar.setOpenMobile(false);

    const redirectTarget =
      localSearchTerm === ""
        ? "/"
        : `/search/${localSearchTerm.replaceAll(" ", "_")}`;
    await goto(redirectTarget, { keepFocus: true });
  };

  let me: HTMLElement | null = $state(null);

  const focusMe = (k: KeyboardEvent) => {
    if (k.key === "/" && me !== document.activeElement) {
      k.preventDefault();
      me?.focus();
    }
  };
</script>

<svelte:window onkeypress={focusMe} />

<!-- svelte-ignore a11y_no_noninteractive_tabindex -->
<!-- This tabindex is necessary to not have this form become automatically
 selected when unhiding the sidebar on mobile, making the keyboard pop up
 every time. -->
<form onsubmit={submit} tabindex={0}>
  <Input
    class="border-none"
    type="text"
    placeholder="Search"
    bind:value={localSearchTerm}
    bind:ref={me}
  />
</form>
