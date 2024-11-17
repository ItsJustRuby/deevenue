<script lang="ts">
  import Lock from "lucide-svelte/icons/lock";
  import * as Sidebar from "$lib/components/ui/sidebar/index.js";

  import DeevenueSidebar from "$lib/components/sidebar/deevenue-sidebar.svelte";
  import NotificationsBox from "$lib/components/notifications-box.svelte";

  import "../app.css";
  import { initializeTitleParts } from "$lib/title.svelte";
  import Spinner from "$lib/components/spinner.svelte";
  import { api } from "$lib/api/client";
  let { children } = $props();

  let titleParts = $state(["Deevenue"]);
  initializeTitleParts(titleParts);

  let isBackendHealthy = $state<boolean | null>(null);
  let isBackendForbidden = $state<boolean | null>(null);
  let doShowSpinner = $state<boolean>(false);

  const checkBackendHealth = () => {
    api
      .GET("/health")
      .then((res) => {
        // This may happen if your auth is a bit weird
        // and the frontend is accessible, but the backend is not.
        if (res.response.status === 403) {
          isBackendForbidden = true;
          return;
        }

        if (res.error) {
          enqueueHealthCheck();
          return;
        }

        if (res.data.status === "pass") isBackendHealthy = true;
        else {
          isBackendHealthy = false;
          enqueueHealthCheck();
        }
      })
      .catch(() => enqueueHealthCheck());
  };

  const enqueueHealthCheck = () => {
    setTimeout(() => checkBackendHealth(), 200);
  };

  // Reduce flashing by only showing the spinner after X ms
  // of not knowing the backend's health, and showing nothing at all before then.
  checkBackendHealth();
  setTimeout(() => {
    if (!isBackendHealthy) doShowSpinner = true;
  }, 2000);
</script>

<svelte:head>
  <title>{titleParts.join(" - ")}</title>
  <meta name="description" content="Deevenue image booru" />
</svelte:head>

{#if isBackendHealthy}
  <div class="app">
    <Sidebar.Provider>
      <DeevenueSidebar />
      <div class="w-full">
        <header
          class="sticky left-2 top-2 w-min rounded-full bg-accent md:hidden"
        >
          <Sidebar.Trigger />
        </header>
        <main>
          <NotificationsBox />
          {@render children?.()}
        </main>
      </div>
    </Sidebar.Provider>
  </div>
{:else}
  <div class="flex h-screen flex-row items-center justify-center">
    {#if isBackendForbidden}
      <Lock class="size-1/4" />
    {:else if doShowSpinner}
      <Spinner class="size-1/4" />
    {/if}
  </div>
{/if}

<style>
  .app {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }

  main {
    flex: 1;
    display: flex;
    flex-direction: column;
    padding: 1rem;
    width: 100%;
    margin: 0 auto;
    box-sizing: border-box;
  }
</style>
