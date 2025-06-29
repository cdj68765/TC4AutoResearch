package com.Emil.TCAutoResearch;

import net.minecraft.entity.player.EntityPlayer;
import net.minecraft.world.World;

import cpw.mods.fml.common.network.IGuiHandler;
import thaumcraft.client.gui.GuiResearchTable;
import thaumcraft.common.container.ContainerResearchTable;
import thaumcraft.common.tiles.TileResearchTable;

public class GuiHandler implements IGuiHandler {

    private final IGuiHandler guiHandler;

    public GuiHandler(IGuiHandler guiHandler) {
        this.guiHandler = guiHandler;
    }

    @Override
    public Object getServerGuiElement(int ID, EntityPlayer player, World world, int x, int y, int z) {
        if (true) {
            if (ID == 0)
                return new ContainerResearchTable(player.inventory, (TileResearchTable) world.getTileEntity(x, y, z));

            return null;
        } else return guiHandler.getServerGuiElement(ID, player, world, x, y, z);
    }

    @Override
    public Object getClientGuiElement(int ID, EntityPlayer player, World world, int x, int y, int z) {
        if (true) {
            if (ID == 0) return new GuiResearchTable(player, (TileResearchTable) world.getTileEntity(x, y, z));
            return null;
        } else return guiHandler.getClientGuiElement(ID, player, world, x, y, z);
    }
}
