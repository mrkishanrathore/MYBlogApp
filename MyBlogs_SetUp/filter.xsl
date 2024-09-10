<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" exclude-result-prefixes="xsl wix"
                xmlns:wix="http://wixtoolset.org/schemas/v4/wxs"
                xmlns="http://wixtoolset.org/schemas/v4/wxs">

	<xsl:output method="xml" indent="yes" omit-xml-declaration="yes" />

	<xsl:strip-space elements="*" />

	<!-- Key to filter out only MyBlogsRunner.exe -->
	<xsl:key name="FilterMyBlogsRunner" match="wix:Component[wix:File[contains(@Source, 'MyBlogsRunner.exe')]]" use="@Id" />

	<!-- Copy all elements and their attributes. -->
	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" />
		</xsl:copy>
	</xsl:template>

	<!-- Exclude only MyBlogsRunner.exe -->
	<xsl:template match="*[ self::wix:Component or self::wix:ComponentRef ][ key( 'FilterMyBlogsRunner', @Id ) ]" />

</xsl:stylesheet>
