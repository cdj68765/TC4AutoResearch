package com.Emil.TCAutoResearch.proxy;

import cpw.mods.fml.common.event.FMLLoadCompleteEvent;
import cpw.mods.fml.common.event.FMLPostInitializationEvent;
import cpw.mods.fml.common.event.FMLPreInitializationEvent;

public interface IProxy {

    default void postInit(FMLPostInitializationEvent event) {}

    default void complete(FMLLoadCompleteEvent event) {}

    void preInit(FMLPreInitializationEvent event);
}
