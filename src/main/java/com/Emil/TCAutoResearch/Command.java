package com.Emil.TCAutoResearch;

import net.minecraft.command.CommandBase;
import net.minecraft.command.ICommandSender;

public class Command extends CommandBase {

    @Override
    public int getRequiredPermissionLevel() {
        return 0;
    }

    @Override
    public String getCommandName() {
        return "TCAutoResearch";
    }

    @Override
    public String getCommandUsage(ICommandSender sender) {
        return "TCAutoResearch";
    }

    @Override
    public void processCommand(ICommandSender sender, String[] args) {

    }
}
