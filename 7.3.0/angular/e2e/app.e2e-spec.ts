import { CourseFeedbackMSTemplatePage } from './app.po';

describe('CourseFeedbackMS App', function() {
  let page: CourseFeedbackMSTemplatePage;

  beforeEach(() => {
    page = new CourseFeedbackMSTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
