# Gene Tools

## Custom BodyType Genes

To assign a custom bodytype to a gene add the mod extension to  your genedef:

    <modExtensions>
		<li Class="GeneTools.GeneToolsGeneDef">
			<forcedBodyTypes>
				<li>YourAndrogynousBody</li>
				<li>YourMaleBody</li>
			</forcedBodyTypes>
			<forcedBodyTypesFemale>
				<li>YourAndrogynousBody</li>
				<li>YourFemaleBody</li>
			</forcedBodyTypesFemale>
		</li>
	</modExtensions>
You can list as many bodytypes as you like and one will be chosen at random for a pawn with the gene. Any adult will use `<forcedBodyTypes>` but if you list any under `<forcedBodyTypesFemale>` females will only use those. The same applies for `<forcedBodyTypesChild>` and `<forcedBodyTypesBaby>` which you can also use.
## Coloring Bodies and Heads

By default all bodies and heads are colored by the pawns skin color. If you want to work around this you can either prevent all colorization and just use the original texture's coloring, or use a color mask to draw where on your body you would like to have skin/hair/no color. For the former, you need to use the following mod extension in your bodyTypeDef:
 

    <modExtensions>
		<li Class="GeneTools.GeneToolsBodyTypeDef">
			<colorBody>false</colorBody>
		</li>
	</modExtensions>
and to do the same for the head, add this to the headTypeDef:

    <modExtensions>
		<li Class="GeneTools.GeneToolsHeadTypeDef">
			<colorHead>false</colorHead>
		</li>
	</modExtensions>
Now if you want to use a mask instead, simply replace "colorHead/colorBody" with `<useShader>true</useShader>` and make sure to have the texture mask in the same folder as the rest of the head/body's textures.
## Having Your Custom Bodies Wear Clothes

This mod restricts pawns from wearing/spawning with apparel that has not been allowed for their body type. By default all apparel can be worn by the vanilla body types only. So, if you want your apparel to be restricted to your custom body, or you want your custom body to be able to wear vanilla apparel, you need to do some patching.

    <modExtensions>
		<li Class="GeneTools.GeneToolsApparelDef">
			<allowedBodyTypes>
				<li>Hulk</li>
			</allowedBodyTypes>
			<forcedBodyTypes>
				<li>Hulk</li>
			</forcedBodyTypes>
		</li>
	</modExtensions>
When added to an apparel ThingDef, the above mod extension will allow only pawns with the hulk bodytype to wear it. Note that it has to be both forced and allowed. Remember that all vanilla bodies are allowed by default, so `<forcedBodyTypes>` acts as a kind of second whitelist.

If you want to allow your custom bodytype to wear a piece of vanilla apparel, you just have to patch that apparel like so:

    <Operation Class="PatchOperationAddModExtension">
	<xpath>/Defs/ThingDef[defName="Apparel_Parka"]</xpath>
	<value>
		<li Class="GeneTools.GeneToolsApparelDef">
			<allowedBodyTypes>
				<li>MyBodyType</li>
			</allowedBodyTypes>
		</li>
	</value>
	</Operation>
Just make sure that you have created a texture for that apparel, named properly (Parka_MyBodyType_south etc.), and in the proper folder, otherwise there will be errors.