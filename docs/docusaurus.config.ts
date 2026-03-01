import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Tombatron.Turbo',
  tagline: 'Hotwire Turbo for ASP.NET Core with SignalR-powered real-time streams',
  favicon: 'img/favicon.ico',

  url: 'https://tombatron.github.io',
  baseUrl: '/Tombatron.Turbo/',

  organizationName: 'tombatron',
  projectName: 'Tombatron.Turbo',

  onBrokenLinks: 'throw',

  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/tombatron/Tombatron.Turbo/tree/main/docs/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    navbar: {
      title: 'Tombatron.Turbo',
      logo: {
        alt: 'Tombatron.Turbo Logo',
        src: 'img/logo.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Docs',
        },
        {
          href: 'https://www.nuget.org/packages/Tombatron.Turbo/',
          label: 'NuGet',
          position: 'right',
        },
        {
          href: 'https://github.com/tombatron/Tombatron.Turbo',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'Getting Started',
              to: '/docs/getting-started',
            },
            {
              label: 'Tutorials',
              to: '/docs/tutorials/todo-list',
            },
            {
              label: 'API Reference',
              to: '/docs/api/ITurbo',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/tombatron/Tombatron.Turbo',
            },
            {
              label: 'NuGet',
              href: 'https://www.nuget.org/packages/Tombatron.Turbo/',
            },
            {
              label: 'npm',
              href: 'https://www.npmjs.com/package/@tombatron/turbo-signalr',
            },
          ],
        },
      ],
      copyright: `Copyright \u00a9 ${new Date().getFullYear()} Tombatron. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'json', 'bash'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
