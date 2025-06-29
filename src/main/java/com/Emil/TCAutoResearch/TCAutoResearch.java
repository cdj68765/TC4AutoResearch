package com.Emil.TCAutoResearch;

import static com.Emil.TCAutoResearch.Config.config;
import static com.Emil.TCAutoResearch.Config.synchronizeConfiguration;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import com.Emil.TCAutoResearch.proxy.IProxy;

import cpw.mods.fml.client.event.ConfigChangedEvent;
import cpw.mods.fml.common.Mod;
import cpw.mods.fml.common.SidedProxy;
import cpw.mods.fml.common.event.FMLLoadCompleteEvent;
import cpw.mods.fml.common.event.FMLPostInitializationEvent;
import cpw.mods.fml.common.event.FMLPreInitializationEvent;

@Mod(
    modid = TCAutoResearch.MODID,
    version = "",
    name = "TCAutoResearch",
    acceptedMinecraftVersions = "[1.7.10]",
    dependencies = "after:ThaumcraftResearchTweaks")
public class TCAutoResearch {

    public static final String MODID = "TCAutoResearchByEmil";
    public static final Logger LOG = LogManager.getLogger(MODID);

    @SidedProxy(
        clientSide = "com.Emil.TCAutoResearch.proxy.ClientProxy",
        serverSide = "com.Emil.TCAutoResearch.proxy.ServerProxy")
    public static IProxy proxy;

    @Mod.EventHandler
    // preInit "Run before anything else. Read your config, create blocks, items, etc, and register them with the
    // GameRegistry." (Remove if not needed)
    public void preInit(FMLPreInitializationEvent event) {
        proxy.preInit(event);
    }

    @Mod.EventHandler
    // postInit "Handle interaction with other mods, complete your setup based on this." (Remove if not needed)
    public void postInit(FMLPostInitializationEvent event) {
        proxy.postInit(event);
    }

    @Mod.EventHandler
    // register server commands in this event handler (Remove if not needed)
    public void complete(FMLLoadCompleteEvent event) {
        proxy.complete(event);
    }

    @Mod.EventHandler
    public void onConfigChanged(ConfigChangedEvent.OnConfigChangedEvent event) {
        if (event.modID.equals(MODID)) {
            synchronizeConfiguration(config.getConfigFile());
        }
    }
}
