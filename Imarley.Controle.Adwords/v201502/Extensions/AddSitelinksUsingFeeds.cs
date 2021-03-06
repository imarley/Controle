// Copyright 2015, Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Author: api.anash@gmail.com (Anash P. Oommen)

using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.v201502;

using System;
using System.Collections.Generic;
using System.IO;

namespace Google.Api.Ads.AdWords.Examples.CSharp.v201502 {

  /// <summary>
  /// This code example adds sitelinks to a campaign using feed services.
  /// To create a campaign, run AddCampaign.cs. To add sitelinks using the
  /// simpler ExtensionSetting services, see AddSitelinks.cs.
  ///
  /// Tags: CampaignFeedService.mutate, FeedService.mutate, FeedItemService.mutate,
  /// Tags: FeedMappingService.mutate
  /// </summary>
  public class AddSitelinksUsingFeeds : ExampleBase {

    /// <summary>
    /// Holds data about sitelinks in a feed.
    /// </summary>
    class SitelinksDataHolder {

      /// <summary>
      /// The sitelink feed item IDs.
      /// </summary>
      List<long> feedItemIds = new List<long>();

      /// <summary>
      /// Gets the sitelink feed item IDs.
      /// </summary>
      public List<long> FeedItemIds {
        get {
          return feedItemIds;
        }
      }

      /// <summary>
      /// Gets or sets the feed ID.
      /// </summary>
      public long FeedId {
        get;
        set;
      }

      /// <summary>
      /// Gets or sets the link text feed attribute ID.
      /// </summary>
      public long LinkTextFeedAttributeId {
        get;
        set;
      }

      /// <summary>
      /// Gets or sets the link URL feed attribute ID.
      /// </summary>
      public long LinkFinalUrlFeedAttributeId {
        get;
        set;
      }
    }

    /// <summary>
    /// Main method, to run this code example as a standalone application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args) {
      AddSitelinksUsingFeeds codeExample = new AddSitelinksUsingFeeds();
      Console.WriteLine(codeExample.Description);
      try {
        long campaignId = long.Parse("INSERT_CAMPAIGN_ID_HERE");
        string feedName = "INSERT_FEED_NAME_HERE";
        codeExample.Run(new AdWordsUser(), campaignId, feedName);
      } catch (Exception ex) {
        Console.WriteLine("An exception occurred while running this code example. {0}",
            ExampleUtilities.FormatException(ex));
      }
    }

    /// <summary>
    /// Returns a description about the code example.
    /// </summary>
    public override string Description {
      get {
        return "This code example adds sitelinks to a campaign using feed services. To create a " +
            "campaign, run AddCampaign.cs. To add sitelinks using the simpler ExtensionSetting " +
            "services, see AddSitelinks.cs.";
      }
    }

    /// <summary>
    /// Runs the code example.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="campaignId">Id of the campaign with which sitelinks are associated.
    /// </param>
    /// <param name="feedName">Name of the feed to be created.</param>
    public void Run(AdWordsUser user, long campaignId, string feedName) {
      SitelinksDataHolder sitelinksData = new SitelinksDataHolder();
      createSitelinksFeed(user, sitelinksData, feedName);
      createSitelinksFeedItems(user, sitelinksData);
      createSitelinksFeedMapping(user, sitelinksData);
      createSitelinksCampaignFeed(user, sitelinksData, campaignId);
    }

    private static void createSitelinksFeed(AdWordsUser user, SitelinksDataHolder sitelinksData,
        string feedName) {
      // Get the FeedService.
      FeedService feedService = (FeedService) user.GetService(AdWordsService.v201502.FeedService);

      // Create attributes.
      FeedAttribute textAttribute = new FeedAttribute();
      textAttribute.type = FeedAttributeType.STRING;
      textAttribute.name = "Link Text";
      FeedAttribute finalUrlAttribute = new FeedAttribute();
      finalUrlAttribute.type = FeedAttributeType.URL_LIST;
      finalUrlAttribute.name = "Link URL";

      // Create the feed.
      Feed sitelinksFeed = new Feed();
      sitelinksFeed.name = feedName;
      sitelinksFeed.attributes = new FeedAttribute[] {textAttribute, finalUrlAttribute};
      sitelinksFeed.origin = FeedOrigin.USER;

      // Create operation.
      FeedOperation operation = new FeedOperation();
      operation.operand = sitelinksFeed;
      operation.@operator = Operator.ADD;

      // Add the feed.
      FeedReturnValue result = feedService.mutate(new FeedOperation[] {operation});

      Feed savedFeed = result.value[0];
      sitelinksData.FeedId = savedFeed.id;
      FeedAttribute[] savedAttributes = savedFeed.attributes;
      sitelinksData.LinkTextFeedAttributeId = savedAttributes[0].id;
      sitelinksData.LinkFinalUrlFeedAttributeId = savedAttributes[1].id;
      Console.WriteLine("Feed with name {0} and ID {1} with linkTextAttributeId {2}"
          + " and linkFinalUrlAttributeId {3} was created.", savedFeed.name, savedFeed.id,
          savedAttributes[0].id, savedAttributes[1].id);
    }

    private static void createSitelinksFeedItems(
        AdWordsUser user, SitelinksDataHolder siteLinksData) {
      // Get the FeedItemService.
      FeedItemService feedItemService =
        (FeedItemService) user.GetService(AdWordsService.v201502.FeedItemService);

      // Create operations to add FeedItems.
      FeedItemOperation home =
          newSitelinkFeedItemAddOperation(siteLinksData,
          "Home", "http://www.example.com");
      FeedItemOperation stores =
          newSitelinkFeedItemAddOperation(siteLinksData,
          "Stores", "http://www.example.com/stores");
      FeedItemOperation onSale =
          newSitelinkFeedItemAddOperation(siteLinksData,
          "On Sale", "http://www.example.com/sale");
      FeedItemOperation support =
          newSitelinkFeedItemAddOperation(siteLinksData,
          "Support", "http://www.example.com/support");
      FeedItemOperation products =
          newSitelinkFeedItemAddOperation(siteLinksData,
          "Products", "http://www.example.com/prods");
      FeedItemOperation aboutUs =
          newSitelinkFeedItemAddOperation(siteLinksData,
          "About Us", "http://www.example.com/about");

      FeedItemOperation[] operations =
          new FeedItemOperation[] {home, stores, onSale, support, products, aboutUs};

      FeedItemReturnValue result = feedItemService.mutate(operations);
      foreach (FeedItem item in result.value) {
        Console.WriteLine("FeedItem with feedItemId {0} was added.", item.feedItemId);
        siteLinksData.FeedItemIds.Add(item.feedItemId);
      }
    }

    // See the Placeholder reference page for a list of all the placeholder types and fields.
    // https://developers.google.com/adwords/api/docs/appendix/placeholders.html
    private const int PLACEHOLDER_SITELINKS = 1;

    // See the Placeholder reference page for a list of all the placeholder types and fields.
    private const int PLACEHOLDER_FIELD_SITELINK_LINK_TEXT = 1;
    private const int PLACEHOLDER_FIELD_SITELINK_FINAL_URL = 5;

    private static void createSitelinksFeedMapping(
        AdWordsUser user, SitelinksDataHolder sitelinksData) {
      // Get the FeedItemService.
      FeedMappingService feedMappingService =
        (FeedMappingService) user.GetService(AdWordsService.v201502.FeedMappingService);

      // Map the FeedAttributeIds to the fieldId constants.
      AttributeFieldMapping linkTextFieldMapping = new AttributeFieldMapping();
      linkTextFieldMapping.feedAttributeId = sitelinksData.LinkTextFeedAttributeId;
      linkTextFieldMapping.fieldId = PLACEHOLDER_FIELD_SITELINK_LINK_TEXT;
      AttributeFieldMapping linkFinalUrlFieldMapping = new AttributeFieldMapping();
      linkFinalUrlFieldMapping.feedAttributeId = sitelinksData.LinkFinalUrlFeedAttributeId;
      linkFinalUrlFieldMapping.fieldId = PLACEHOLDER_FIELD_SITELINK_FINAL_URL;

      // Create the FieldMapping and operation.
      FeedMapping feedMapping = new FeedMapping();
      feedMapping.placeholderType = PLACEHOLDER_SITELINKS;
      feedMapping.feedId = sitelinksData.FeedId;
      feedMapping.attributeFieldMappings =
          new AttributeFieldMapping[] {linkTextFieldMapping, linkFinalUrlFieldMapping};
      FeedMappingOperation operation = new FeedMappingOperation();
      operation.operand = feedMapping;
      operation.@operator = Operator.ADD;

      // Save the field mapping.
      FeedMappingReturnValue result =
          feedMappingService.mutate(new FeedMappingOperation[] {operation});
      foreach (FeedMapping savedFeedMapping in result.value) {
        Console.WriteLine(
            "Feed mapping with ID {0} and placeholderType {1} was saved for feed with ID {2}.",
            savedFeedMapping.feedMappingId, savedFeedMapping.placeholderType,
            savedFeedMapping.feedId);
      }
    }

    private static void createSitelinksCampaignFeed(AdWordsUser user,
      SitelinksDataHolder sitelinksData, long campaignId) {
      // Get the CampaignFeedService.
      CampaignFeedService campaignFeedService =
        (CampaignFeedService) user.GetService(AdWordsService.v201502.CampaignFeedService);

      // Map the feed item ids to the campaign using an IN operation.
      RequestContextOperand feedItemRequestContextOperand = new RequestContextOperand();
      feedItemRequestContextOperand.contextType = RequestContextOperandContextType.FEED_ITEM_ID;

      List<FunctionArgumentOperand> feedItemOperands = new List<FunctionArgumentOperand>();
      foreach (long feedItemId in sitelinksData.FeedItemIds) {
        ConstantOperand feedItemOperand = new ConstantOperand();
        feedItemOperand.longValue = feedItemId;
        feedItemOperand.type = ConstantOperandConstantType.LONG;
        feedItemOperands.Add(feedItemOperand);
      }

      Function feedItemfunction = new Function();
      feedItemfunction.lhsOperand = new FunctionArgumentOperand[] {feedItemRequestContextOperand};
      feedItemfunction.@operator = FunctionOperator.IN;
      feedItemfunction.rhsOperand = feedItemOperands.ToArray();

      // Optional: to target to a platform, define a function and 'AND' it with
      // the feed item ID link:
      RequestContextOperand platformRequestContextOperand = new RequestContextOperand();
      platformRequestContextOperand.contextType = RequestContextOperandContextType.DEVICE_PLATFORM;

      ConstantOperand platformOperand = new ConstantOperand();
      platformOperand.stringValue = "Mobile";
      platformOperand.type = ConstantOperandConstantType.STRING;

      Function platformFunction = new Function();
      platformFunction.lhsOperand = new FunctionArgumentOperand[] {platformRequestContextOperand};
      platformFunction.@operator = FunctionOperator.EQUALS;
      platformFunction.rhsOperand = new FunctionArgumentOperand[] {platformOperand};

      // Combine the two functions using an AND operation.
      FunctionOperand feedItemFunctionOperand = new FunctionOperand();
      feedItemFunctionOperand.value = feedItemfunction;

      FunctionOperand platformFunctionOperand = new FunctionOperand();
      platformFunctionOperand.value = platformFunction;

      Function combinedFunction = new Function();
      combinedFunction.@operator = FunctionOperator.AND;
      combinedFunction.lhsOperand = new FunctionArgumentOperand[] {
          feedItemFunctionOperand, platformFunctionOperand};

      CampaignFeed campaignFeed = new CampaignFeed();
      campaignFeed.feedId = sitelinksData.FeedId;
      campaignFeed.campaignId = campaignId;
      campaignFeed.matchingFunction = combinedFunction;
      // Specifying placeholder types on the CampaignFeed allows the same feed
      // to be used for different placeholders in different Campaigns.
      campaignFeed.placeholderTypes = new int[] {PLACEHOLDER_SITELINKS};

      CampaignFeedOperation operation = new CampaignFeedOperation();
      operation.operand = campaignFeed;
      operation.@operator = Operator.ADD;
      CampaignFeedReturnValue result =
          campaignFeedService.mutate(new CampaignFeedOperation[] {operation});
      foreach (CampaignFeed savedCampaignFeed in result.value) {
        Console.WriteLine("Campaign with ID {0} was associated with feed with ID {1}",
            savedCampaignFeed.campaignId, savedCampaignFeed.feedId);
      }
    }

    private static FeedItemOperation newSitelinkFeedItemAddOperation(
        SitelinksDataHolder sitelinksData, String text, String finalUrl) {
      // Create the FeedItemAttributeValues for our text values.
      FeedItemAttributeValue linkTextAttributeValue = new FeedItemAttributeValue();
      linkTextAttributeValue.feedAttributeId = sitelinksData.LinkTextFeedAttributeId;
      linkTextAttributeValue.stringValue = text;
      FeedItemAttributeValue linkFinalUrlAttributeValue = new FeedItemAttributeValue();
      linkFinalUrlAttributeValue.feedAttributeId = sitelinksData.LinkFinalUrlFeedAttributeId;
      linkFinalUrlAttributeValue.stringValues = new string[] { finalUrl };

      // Create the feed item and operation.
      FeedItem item = new FeedItem();
      item.feedId = sitelinksData.FeedId;
      item.attributeValues =
          new FeedItemAttributeValue[] {linkTextAttributeValue, linkFinalUrlAttributeValue};
      FeedItemOperation operation = new FeedItemOperation();
      operation.operand = item;
      operation.@operator = Operator.ADD;
      return operation;
    }
  }
}
