package com.Emil.TCAutoResearch;

import java.util.ArrayList;
import java.util.List;
import java.util.Set;

import com.gtnewhorizon.gtnhmixins.ILateMixinLoader;
import com.gtnewhorizon.gtnhmixins.LateMixin;

import cpw.mods.fml.common.ModContainer;

@LateMixin
public class LateLoader implements ILateMixinLoader {

    @Override
    public String getMixinConfig() {
        return "mixins.TCAutoResearchByEmil.late.json";
    }

    @Override
    public List<String> getMixins(Set<String> loadedMods) {
        List<String> mixins = new ArrayList<>();
        mixins.add("GuiResearchTableMixin");
        return mixins;
    }

    static ModContainer thaumcraftResearchTweaks;
}
