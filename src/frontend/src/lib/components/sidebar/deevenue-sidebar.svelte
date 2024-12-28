<script lang="ts">
  import * as Sidebar from "$lib/components/ui/sidebar/index.js";
  import BicepsFlexed from "lucide-svelte/icons/biceps-flexed";
  import Gavel from "lucide-svelte/icons/gavel";
  import HardHat from "lucide-svelte/icons/hard-hat";
  import Tag from "lucide-svelte/icons/tag";
  import SearchBox from "./search-box.svelte";
  import SfwToggle from "./sfw-toggle.svelte";
  import FileUploadButton from "./file-upload-button.svelte";
  import { getRandomRuleViolation } from "$lib/randomRuleViolation";

  const randomRuleViolation = async (e: Event) => {
    e.preventDefault();
    await getRandomRuleViolation();
    sidebar.setOpenMobile(false);
  };

  const sidebar = Sidebar.useSidebar();
</script>

<Sidebar.Root>
  <Sidebar.Header>
    <a
      class="self-center text-3xl font-bold"
      href="/"
      onclick={() => sidebar.setOpenMobile(false)}><span>Home</span></a
    >
  </Sidebar.Header>
  <Sidebar.Content>
    <Sidebar.Group>
      <Sidebar.GroupContent>
        <Sidebar.Menu>
          <Sidebar.MenuItem>
            <SearchBox />
          </Sidebar.MenuItem>
        </Sidebar.Menu>
      </Sidebar.GroupContent>
    </Sidebar.Group>
    <Sidebar.Group />
    <Sidebar.Group>
      <Sidebar.GroupContent>
        <Sidebar.Menu>
          <Sidebar.MenuItem>
            <FileUploadButton />
          </Sidebar.MenuItem>
        </Sidebar.Menu>
      </Sidebar.GroupContent>
    </Sidebar.Group>
    <Sidebar.Group>
      <Sidebar.GroupLabel>Links</Sidebar.GroupLabel>
      <Sidebar.GroupContent>
        <Sidebar.Menu>
          <Sidebar.MenuItem>
            <Sidebar.MenuButton>
              {#snippet child({ props })}
                <a
                  href="/tags"
                  onclick={() => sidebar.setOpenMobile(false)}
                  {...props}
                >
                  <Tag />
                  <span>Tags</span>
                </a>
              {/snippet}
            </Sidebar.MenuButton>
          </Sidebar.MenuItem>
          <Sidebar.MenuItem>
            <Sidebar.MenuButton>
              {#snippet child({ props })}
                <a
                  href="/rules"
                  onclick={() => sidebar.setOpenMobile(false)}
                  {...props}
                >
                  <Gavel />
                  <span>Rules</span>
                </a>
              {/snippet}
            </Sidebar.MenuButton>
          </Sidebar.MenuItem>
          <Sidebar.MenuItem>
            <Sidebar.MenuButton>
              {#snippet child({ props })}
                <a
                  href="/jobs"
                  onclick={() => sidebar.setOpenMobile(false)}
                  {...props}
                >
                  <HardHat />
                  <span>Jobs</span>
                </a>
              {/snippet}
            </Sidebar.MenuButton>
          </Sidebar.MenuItem>
          <Sidebar.MenuItem>
            <Sidebar.MenuButton>
              {#snippet child({ props })}
                <a
                  href="/rules/violations/random"
                  {...props}
                  onclick={randomRuleViolation}
                >
                  <BicepsFlexed />
                  <span>Random rules violation</span>
                </a>
              {/snippet}
            </Sidebar.MenuButton>
          </Sidebar.MenuItem>
        </Sidebar.Menu>
      </Sidebar.GroupContent>
    </Sidebar.Group>
    <Sidebar.Group />
  </Sidebar.Content>
  <Sidebar.Footer>
    <div class="flex flex-col items-center justify-center">
      <SfwToggle />
      <div class="text-xs text-muted-foreground">{__DEEVENUE_VERSION__}</div>
    </div>
  </Sidebar.Footer>
</Sidebar.Root>
