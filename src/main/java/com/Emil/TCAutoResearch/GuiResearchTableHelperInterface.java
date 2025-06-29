package com.Emil.TCAutoResearch;

import thaumcraft.api.aspects.Aspect;
import thaumcraft.common.lib.utils.HexUtils;

public interface GuiResearchTableHelperInterface {

    void combine(Aspect aspect1, Aspect aspect2);

    void place(HexUtils.Hex hex, Aspect aspect);
}
